using AdmissionSystem.Api.Data;
using AdmissionSystem.Api.DTOs;
using AdmissionSystem.Api.Entities;
using AdmissionSystem.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdmissionSystem.Tests;

public class AdmissionServiceTests
{
    private static AdmissionDbContext CreateContext(string name) => new(new DbContextOptionsBuilder<AdmissionDbContext>()
        .UseInMemoryDatabase(name)
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options);

    [Fact]
    public async Task CreateClassAndReserveSeat_ReturnsHoldAndDecrementsAvailability()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var classes = new ClassService(context);
        var item = await classes.CreateClassAsync(new CreateClassRequest("Class 6 - Morning Shift", 2));

        var service = new AdmissionService(context);
        var hold = await service.CreateHoldAsync(item.Id, "20152691234567890");
        var updated = await classes.GetClassByIdAsync(item.Id);

        Assert.Equal("HELD", hold.Status);
        Assert.Equal(item.Id, hold.ClassId);
        Assert.Equal(1, updated!.AvailableSeats);
        Assert.Equal(1, updated.HeldSeats);
    }

    [Fact]
    public async Task ConfirmationIsIdempotentAndPreventsSecondActiveApplication()
    {
        await using var context = CreateContext(Guid.NewGuid().ToString());
        var classService = new ClassService(context);
        var item = await classService.CreateClassAsync(new CreateClassRequest("Class 7 - Day Shift", 2));
        var admissions = new AdmissionService(context);
        var hold = await admissions.CreateHoldAsync(item.Id, "20152691234567891");

        var first = await admissions.ConfirmHoldAsync(hold.HoldId, hold.StudentId);
        var second = await admissions.ConfirmHoldAsync(hold.HoldId, hold.StudentId);
        var updatedClass = await classService.GetClassByIdAsync(item.Id);

        Assert.Equal("CONFIRMED", first.Status);
        Assert.Equal("CONFIRMED", second.Status);
        Assert.Equal(1, updatedClass!.ConfirmedSeats);
        Assert.Equal(1, updatedClass.AvailableSeats);
        await Assert.ThrowsAsync<InvalidOperationException>(() => admissions.CreateHoldAsync(item.Id, hold.StudentId));
    }

    [Fact]
    public async Task ExpiredHoldIsMarkedAndItsSeatIsReleased()
    {
        await using var context = CreateContext(Guid.NewGuid().ToString());
        var item = new ClassOffering { Id = Guid.NewGuid(), ClassName = "Class 8", TotalSeats = 1, AvailableSeats = 0 };
        var hold = new AdmissionHold { Id = Guid.NewGuid(), ClassOfferingId = item.Id, ClassOffering = item, StudentId = "20152691234567892", ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1), Status = "HELD" };
        context.AddRange(item, hold);
        await context.SaveChangesAsync();

        await new AdmissionService(context).ExpireAllAsync();

        Assert.Equal("EXPIRED", (await context.AdmissionHolds.SingleAsync()).Status);
        Assert.Equal(1, (await context.ClassOfferings.SingleAsync()).AvailableSeats);
    }
}

public sealed class PostgresRequiredFactAttribute : FactAttribute
{
    public PostgresRequiredFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_DATABASE_URL")))
            Skip = "Set TEST_DATABASE_URL to a disposable PostgreSQL database to run the row-locking concurrency test.";
    }
}

public class ReservationConcurrencyTests
{
    [PostgresRequiredFact]
    public async Task ConcurrentReservationsNeverExceedCapacity()
    {
        var connection = Environment.GetEnvironmentVariable("TEST_DATABASE_URL")!;
        var options = new DbContextOptionsBuilder<AdmissionDbContext>().UseNpgsql(connection).Options;
        var classId = Guid.NewGuid();
        await using (var setup = new AdmissionDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.ClassOfferings.Add(new ClassOffering { Id = classId, ClassName = "Concurrency Test", TotalSeats = 10, AvailableSeats = 10 });
            await setup.SaveChangesAsync();
        }

        var results = await Task.WhenAll(Enumerable.Range(0, 100).Select(async index =>
        {
            await using var db = new AdmissionDbContext(options);
            try
            {
                await new AdmissionService(db).CreateHoldAsync(classId, $"{10000000 + index}");
                return true;
            }
            catch (SeatUnavailableException) { return false; }
            catch (InvalidOperationException) { return false; }
        }));

        await using (var verify = new AdmissionDbContext(options))
        {
            Assert.Equal(10, results.Count(x => x));
            var item = await verify.ClassOfferings.SingleAsync(c => c.Id == classId);
            Assert.Equal(10, await verify.AdmissionHolds.CountAsync(h => h.ClassOfferingId == classId && h.Status == "HELD"));
            Assert.InRange(item.AvailableSeats, 0, item.TotalSeats);
            Assert.Equal(item.TotalSeats, item.AvailableSeats + await verify.AdmissionHolds.CountAsync(h => h.ClassOfferingId == classId && (h.Status == "HELD" || h.Status == "CONFIRMED")));
            verify.Remove(item);
            await verify.SaveChangesAsync();
        }
    }
}
