using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using AdmissionSystem.Api.Data;
using AdmissionSystem.Api.Entities;
using AdmissionSystem.Api.Services;
using AdmissionSystem.Api.DTOs;

namespace AdmissionSystem.Tests
{
    public class AdmissionServiceTests
    {
        private AdmissionDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AdmissionDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new AdmissionDbContext(options);
        }

        [Fact]
        public async Task CreateHold_ShouldDecrementAvailableSeats()
        {
            // Arrange
            using var context = GetDbContext();
            var classOffering = new ClassOffering
            {
                Id = Guid.NewGuid(),
                ClassName = "Test Class",
                TotalSeats = 10,
                AvailableSeats = 10
            };
            context.ClassOfferings.Add(classOffering);
            await context.SaveChangesAsync();

            var service = new AdmissionService(context);
            string studentId = "student123";

            // Act
            var result = await service.CreateHoldAsync(classOffering.Id, studentId);

            // Assert
            result.Should().NotBeNull();
            var updatedClass = await context.ClassOfferings.FindAsync(classOffering.Id);
            updatedClass.AvailableSeats.Should().Be(9);
        }

        [Fact]
        public async Task CreateHold_ShouldFail_WhenNoSeatsAvailable()
        {
            // Arrange
            using var context = GetDbContext();
            var classOffering = new ClassOffering
            {
                Id = Guid.NewGuid(),
                ClassName = "Full Class",
                TotalSeats = 1,
                AvailableSeats = 0
            };
            context.ClassOfferings.Add(classOffering);
            await context.SaveChangesAsync();

            var service = new AdmissionService(context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateHoldAsync(classOffering.Id, "student123"));
        }

        [Fact]
        public async Task ConfirmHold_ShouldBeIdempotent()
        {
            // Arrange
            using var context = GetDbContext();
            var classId = Guid.NewGuid();
            var classOffering = new ClassOffering
            {
                Id = classId,
                ClassName = "Test Class",
                TotalSeats = 10,
                AvailableSeats = 10
            };
            context.ClassOfferings.Add(classOffering);

            var hold = new AdmissionHold
            {
                Id = Guid.NewGuid(),
                ClassOfferingId = classId,
                StudentId = "student123",
                IsConfirmed = false,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            context.AdmissionHolds.Add(hold);
            await context.SaveChangesAsync();

            var service = new AdmissionService(context);

            // Act
            await service.ConfirmHoldAsync(hold.Id, "student123");
            await service.ConfirmHoldAsync(hold.Id, "student123"); // Second call

            // Assert
            var updatedHold = await context.AdmissionHolds.FindAsync(hold.Id);
            updatedHold.IsConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task CreateHold_ShouldReleaseExpiredSeats()
        {
            // Arrange
            using var context = GetDbContext();
            var classId = Guid.NewGuid();
            var classOffering = new ClassOffering
            {
                Id = classId,
                ClassName = "Test Class",
                TotalSeats = 1,
                AvailableSeats = 0
            };
            context.ClassOfferings.Add(classOffering);

            var expiredHold = new AdmissionHold
            {
                Id = Guid.NewGuid(),
                ClassOfferingId = classId,
                StudentId = "oldStudent",
                IsConfirmed = false,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1) // Expired
            };
            context.AdmissionHolds.Add(expiredHold);
            await context.SaveChangesAsync();

            var service = new AdmissionService(context);

            // Act
            var result = await service.CreateHoldAsync(classId, "newStudent");

            // Assert
            result.Should().NotBeNull();
            var updatedClass = await context.ClassOfferings.FindAsync(classId);
            updatedClass.AvailableSeats.Should().Be(0); // 0 + 1 (released) - 1 (new) = 0
        }
    }
}
