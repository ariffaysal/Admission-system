using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AdmissionSystem.Api.Data;
using AdmissionSystem.Api.Entities;
using AdmissionSystem.Api.DTOs;
using AdmissionSystem.Api.Interfaces;

namespace AdmissionSystem.Api.Services
{
    public class AdmissionService : IAdmissionService
    {
        private readonly AdmissionDbContext _context;
        private const int HOLD_DURATION_SECONDS = 600;

        public AdmissionService(AdmissionDbContext context)
        {
            _context = context;
        }

        public async Task<AdmissionHoldDto> CreateHoldAsync(Guid classId, string studentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lock the class row exclusively (Pessimistic Locking)
                var classOffering = await _context.ClassOfferings
                    .FromSqlRaw("SELECT * FROM \"ClassOfferings\" WHERE \"Id\" = {0} FOR UPDATE", classId)
                    .SingleOrDefaultAsync();

                if (classOffering == null) throw new KeyNotFoundException("Class not found.");

                // Just-in-Time Cleanup: Process expired holds for this class
                var expiredHolds = await _context.AdmissionHolds
                    .Where(h => h.ClassOfferingId == classId && !h.IsConfirmed && h.ExpiresAt < DateTimeOffset.UtcNow)
                    .ToListAsync();

                foreach (var expiredHold in expiredHolds)
                {
                    // We don't have a specific EXPIRED status in this simplified boolean model,
                    // so we effectively "cancel" them by releasing the seat.
                    // In a full implementation, we'd update a status column.
                    // To avoid duplicate index issues, we might need to handle this carefully.
                    _context.AdmissionHolds.Remove(expiredHold);
                    classOffering.AvailableSeats++;
                }
                await _context.SaveChangesAsync();

                if (classOffering.AvailableSeats <= 0)
                    throw new InvalidOperationException("No seats available.");

                // Check for active application (Partial Unique Index handles this at DB level,
                // but we catch it here for a better error message).
                var existingHold = await _context.AdmissionHolds
                    .AnyAsync(h => h.StudentId == studentId && h.ClassOfferingId == classId && !h.IsConfirmed);

                if (existingHold)
                    throw new InvalidOperationException("Student already has an active hold for this class.");

                var hold = new AdmissionHold
                {
                    Id = Guid.NewGuid(),
                    ClassOfferingId = classId,
                    StudentId = studentId,
                    IsConfirmed = false,
                    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(HOLD_DURATION_SECONDS)
                };

                classOffering.AvailableSeats--;
                _context.AdmissionHolds.Add(hold);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new AdmissionHoldDto(hold.Id, hold.StudentId, hold.ClassOfferingId, hold.ExpiresAt, hold.IsConfirmed);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AdmissionHoldDto> GetHoldByIdAsync(Guid holdId)
        {
            var hold = await _context.AdmissionHolds
                .Include(h => h.ClassOffering)
                .SingleOrDefaultAsync(h => h.Id == holdId);

            if (hold == null) throw new KeyNotFoundException("Hold not found.");

            return new AdmissionHoldDto(hold.Id, hold.StudentId, hold.ClassOfferingId, hold.ExpiresAt, hold.IsConfirmed);
        }

        public async Task ConfirmHoldAsync(Guid holdId, string studentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hold = await _context.AdmissionHolds
                    .SingleOrDefaultAsync(h => h.Id == holdId);

                if (hold == null) throw new KeyNotFoundException("Hold not found.");
                if (hold.StudentId != studentId) throw new UnauthorizedAccessException("Student does not own this hold.");
                if (hold.IsConfirmed) return; // Idempotent
                if (hold.ExpiresAt < DateTimeOffset.UtcNow) throw new InvalidOperationException("Hold has expired.");

                // Lock class row to update confirmed seats (if we tracked them separately)
                // In our simplified AvailableSeats model, confirmation doesn't change AvailableSeats,
                // only the transition from HELD -> CONFIRMED.

                hold.IsConfirmed = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelHoldAsync(Guid holdId, string studentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hold = await _context.AdmissionHolds.SingleOrDefaultAsync(h => h.Id == holdId);
                if (hold == null) throw new KeyNotFoundException("Hold not found.");
                if (hold.StudentId != studentId) throw new UnauthorizedAccessException("Student does not own this hold.");

                if (!hold.IsConfirmed)
                {
                    // Release seat
                    var classOffering = await _context.ClassOfferings
                        .FromSqlRaw("SELECT * FROM \"ClassOfferings\" WHERE \"Id\" = {0} FOR UPDATE", hold.ClassOfferingId)
                        .SingleOrDefaultAsync();

                    if (classOffering != null)
                    {
                        classOffering.AvailableSeats++;
                    }
                }

                _context.AdmissionHolds.Remove(hold);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<AdmissionHoldDto>> GetHoldsForStudentAsync(string studentId)
        {
            return await _context.AdmissionHolds
                .Where(h => h.StudentId == studentId)
                .Select(h => new AdmissionHoldDto(h.Id, h.StudentId, h.ClassOfferingId, h.ExpiresAt, h.IsConfirmed))
                .ToListAsync();
        }
    }
}
