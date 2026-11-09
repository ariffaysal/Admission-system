using AdmissionSystem.Api.Data;
using AdmissionSystem.Api.DTOs;
using AdmissionSystem.Api.Entities;
using AdmissionSystem.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Api.Services;

public class AdmissionService : IAdmissionService
{
    private readonly AdmissionDbContext _context;
    public AdmissionService(AdmissionDbContext context) => _context = context;

    public async Task<AdmissionHoldDto> CreateHoldAsync(Guid classId, string studentId)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        var c = await LockClass(classId);
        if (c is null) throw new KeyNotFoundException("Class not found.");
        await ExpireClassHolds(c);
        if (await _context.AdmissionHolds.AnyAsync(h => h.ClassOfferingId == classId && h.StudentId == studentId && (h.Status == "HELD" || h.Status == "CONFIRMED")))
            throw new InvalidOperationException("An active application already exists for this student and class.");
        if (c.AvailableSeats <= 0) throw new SeatUnavailableException("No seats are available.");
        var duration = int.TryParse(Environment.GetEnvironmentVariable("HOLD_DURATION_SECONDS"), out var configured) ? Math.Clamp(configured, 1, 86400) : 600;
        var hold = new AdmissionHold { Id = Guid.NewGuid(), ClassOfferingId = classId, StudentId = studentId, Status = "HELD", ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(duration) };
        c.AvailableSeats--;
        _context.AdmissionHolds.Add(hold);
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return ToDto(hold, c.ClassName);
    }

    public async Task<AdmissionHoldDto> GetHoldByIdAsync(Guid holdId, string studentId)
    {
        var hold = await _context.AdmissionHolds.Include(h => h.ClassOffering).SingleOrDefaultAsync(h => h.Id == holdId);
        if (hold is null) throw new KeyNotFoundException("Hold not found.");
        if (hold.StudentId != studentId) throw new UnauthorizedAccessException("Student does not own this application.");
        if (hold.Status == "HELD" && hold.ExpiresAt <= DateTimeOffset.UtcNow) await ExpireById(hold.Id);
        return ToDto(hold, hold.ClassOffering.ClassName);
    }

    public async Task<AdmissionHoldDto> ConfirmHoldAsync(Guid holdId, string studentId)
    {
        var hold = await _context.AdmissionHolds.AsNoTracking().SingleOrDefaultAsync(h => h.Id == holdId);
        if (hold is null) throw new KeyNotFoundException("Hold not found.");
        await using var tx = await _context.Database.BeginTransactionAsync();
        var c = await LockClass(hold.ClassOfferingId);
        if (c is null) throw new KeyNotFoundException("Class not found.");
        hold = await _context.AdmissionHolds.SingleAsync(h => h.Id == holdId);
        if (hold.StudentId != studentId) throw new UnauthorizedAccessException("Student does not own this application.");
        if (hold.Status == "CONFIRMED") { await tx.CommitAsync(); return ToDto(hold, c.ClassName); }
        if (hold.Status != "HELD") throw new InvalidOperationException($"Application is {hold.Status} and cannot be confirmed.");
        if (hold.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            hold.Status = "EXPIRED";
            c.AvailableSeats++;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            throw new HoldExpiredException("Reservation has expired.");
        }
        hold.Status = "CONFIRMED";
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return ToDto(hold, c.ClassName);
    }

    public async Task CancelHoldAsync(Guid holdId, string studentId)
    {
        var hold = await _context.AdmissionHolds.AsNoTracking().SingleOrDefaultAsync(h => h.Id == holdId);
        if (hold is null) throw new KeyNotFoundException("Hold not found.");
        await using var tx = await _context.Database.BeginTransactionAsync();
        var c = await LockClass(hold.ClassOfferingId);
        if (c is null) throw new KeyNotFoundException("Class not found.");
        hold = await _context.AdmissionHolds.SingleAsync(h => h.Id == holdId);
        if (hold.StudentId != studentId) throw new UnauthorizedAccessException("Student does not own this application.");
        if (hold.Status == "HELD") { hold.Status = "CANCELLED"; c.AvailableSeats++; await _context.SaveChangesAsync(); }
        await tx.CommitAsync();
    }

    public async Task<IEnumerable<AdmissionHoldDto>> GetHoldsForStudentAsync(string studentId)
    {
        await ExpireStudentHolds(studentId);
        return await _context.AdmissionHolds.AsNoTracking().Include(h => h.ClassOffering).Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt).Select(h => new AdmissionHoldDto(h.Id, h.StudentId, h.ClassOfferingId, h.ExpiresAt, h.Status, h.ClassOffering.ClassName)).ToListAsync();
    }

    public async Task ExpireAllAsync()
    {
        var ids = await _context.AdmissionHolds.Where(h => h.Status == "HELD" && h.ExpiresAt <= DateTimeOffset.UtcNow).Select(h => h.Id).ToListAsync();
        foreach (var id in ids) await ExpireById(id);
    }

    private async Task ExpireStudentHolds(string studentId)
    {
        var ids = await _context.AdmissionHolds.Where(h => h.StudentId == studentId && h.Status == "HELD" && h.ExpiresAt <= DateTimeOffset.UtcNow).Select(h => h.Id).ToListAsync();
        foreach (var id in ids) await ExpireById(id);
    }

    private async Task ExpireClassHolds(ClassOffering c)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _context.AdmissionHolds.Where(h => h.ClassOfferingId == c.Id && h.Status == "HELD" && h.ExpiresAt <= now).ToListAsync();
        foreach (var hold in expired) hold.Status = "EXPIRED";
        c.AvailableSeats += expired.Count;
        if (expired.Count > 0) await _context.SaveChangesAsync();
    }

    private async Task ExpireById(Guid id)
    {
        var classId = await _context.AdmissionHolds.Where(h => h.Id == id).Select(h => h.ClassOfferingId).SingleOrDefaultAsync();
        if (classId == Guid.Empty) return;
        await using var tx = await _context.Database.BeginTransactionAsync();
        var c = await LockClass(classId);
        if (c is null) { await tx.CommitAsync(); return; }
        var hold = await _context.AdmissionHolds.SingleOrDefaultAsync(h => h.Id == id);
        if (hold is { Status: "HELD" } && hold.ExpiresAt <= DateTimeOffset.UtcNow)
        { hold.Status = "EXPIRED"; c.AvailableSeats++; await _context.SaveChangesAsync(); }
        await tx.CommitAsync();
    }

    private Task<ClassOffering?> LockClass(Guid id) => _context.Database.IsRelational()
        ? _context.ClassOfferings.FromSqlInterpolated($"SELECT * FROM \"ClassOfferings\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync()
        : _context.ClassOfferings.SingleOrDefaultAsync(c => c.Id == id);
    private static AdmissionHoldDto ToDto(AdmissionHold h, string className) => new(h.Id, h.StudentId, h.ClassOfferingId, h.ExpiresAt, h.Status, className);
}

public sealed class SeatUnavailableException(string message) : Exception(message);
public sealed class HoldExpiredException(string message) : Exception(message);
