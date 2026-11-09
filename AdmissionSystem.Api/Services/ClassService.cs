using AdmissionSystem.Api.Data;
using AdmissionSystem.Api.DTOs;
using AdmissionSystem.Api.Entities;
using AdmissionSystem.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Api.Services;

public class ClassService : IClassService
{
    private readonly AdmissionDbContext _context;
    public ClassService(AdmissionDbContext context) => _context = context;

    public async Task<ClassOfferingDto> CreateClassAsync(CreateClassRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 255)
            throw new ArgumentException("Class name is required and must be at most 255 characters.");
        if (request.TotalSeats <= 0) throw new ArgumentException("Total seats must be greater than zero.");
        var item = new ClassOffering { Id = Guid.NewGuid(), ClassName = request.Name.Trim(), TotalSeats = request.TotalSeats, AvailableSeats = request.TotalSeats };
        _context.ClassOfferings.Add(item);
        await _context.SaveChangesAsync();
        return await ToDto(item);
    }

    public async Task<IEnumerable<ClassOfferingDto>> GetAllClassesAsync()
    {
        await ExpireAllAsync();
        var classes = await _context.ClassOfferings.AsNoTracking().OrderBy(c => c.ClassName).ToListAsync();
        var counts = await _context.AdmissionHolds.AsNoTracking().Where(h => h.Status == "HELD" || h.Status == "CONFIRMED")
            .GroupBy(h => new { h.ClassOfferingId, h.Status }).Select(g => new { g.Key.ClassOfferingId, g.Key.Status, Count = g.Count() }).ToListAsync();
        return classes.Select(c => new ClassOfferingDto(c.Id, c.ClassName, c.TotalSeats,
            counts.Where(x => x.ClassOfferingId == c.Id && x.Status == "HELD").Sum(x => x.Count),
            counts.Where(x => x.ClassOfferingId == c.Id && x.Status == "CONFIRMED").Sum(x => x.Count), c.AvailableSeats));
    }

    public async Task<ClassOfferingDto?> GetClassByIdAsync(Guid id)
    {
        await ExpireAllAsync();
        var c = await _context.ClassOfferings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        return c is null ? null : await ToDto(c);
    }

    private async Task<ClassOfferingDto> ToDto(ClassOffering c)
    {
        var grouped = await _context.AdmissionHolds.AsNoTracking().Where(h => h.ClassOfferingId == c.Id && (h.Status == "HELD" || h.Status == "CONFIRMED"))
            .GroupBy(h => h.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
        var held = grouped.FirstOrDefault(x => x.Status == "HELD")?.Count ?? 0;
        var confirmed = grouped.FirstOrDefault(x => x.Status == "CONFIRMED")?.Count ?? 0;
        return new(c.Id, c.ClassName, c.TotalSeats, held, confirmed, c.AvailableSeats);
    }

    internal async Task ExpireAllAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var classIds = await _context.AdmissionHolds.Where(h => h.Status == "HELD" && h.ExpiresAt <= now).Select(h => h.ClassOfferingId).Distinct().ToListAsync();
        foreach (var classId in classIds)
        {
            await using var tx = _context.Database.IsRelational() ? await _context.Database.BeginTransactionAsync() : null;
            var c = _context.Database.IsRelational()
                ? await _context.ClassOfferings.FromSqlInterpolated($"SELECT * FROM \"ClassOfferings\" WHERE \"Id\" = {classId} FOR UPDATE").SingleOrDefaultAsync()
                : await _context.ClassOfferings.SingleOrDefaultAsync(x => x.Id == classId);
            if (c is null) { if (tx is not null) await tx.CommitAsync(); continue; }
            var expired = await _context.AdmissionHolds.Where(h => h.ClassOfferingId == classId && h.Status == "HELD" && h.ExpiresAt <= now).ToListAsync();
            foreach (var hold in expired) hold.Status = "EXPIRED";
            c.AvailableSeats += expired.Count;
            if (expired.Count > 0) await _context.SaveChangesAsync();
            if (tx is not null) await tx.CommitAsync();
        }
    }
}
