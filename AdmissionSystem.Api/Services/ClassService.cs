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
    public class ClassService : IClassService
    {
        private readonly AdmissionDbContext _context;

        public ClassService(AdmissionDbContext context)
        {
            _context = context;
        }

        public async Task<ClassOfferingDto> CreateClassAsync(CreateClassRequest request)
        {
            if (request.TotalSeats <= 0)
                throw new ArgumentException("Total seats must be greater than zero.");

            var classOffering = new ClassOffering
            {
                Id = Guid.NewGuid(),
                ClassName = request.ClassName,
                TotalSeats = request.TotalSeats,
                AvailableSeats = request.TotalSeats
            };

            _context.ClassOfferings.Add(classOffering);
            await _context.SaveChangesAsync();

            return new ClassOfferingDto(classOffering.Id, classOffering.ClassName, classOffering.TotalSeats, classOffering.AvailableSeats);
        }

        public async Task<IEnumerable<ClassOfferingDto>> GetAllClassesAsync()
        {
            return await _context.ClassOfferings
                .Select(c => new ClassOfferingDto(c.Id, c.ClassName, c.TotalSeats, c.AvailableSeats))
                .ToListAsync();
        }

        public async Task<ClassOfferingDto> GetClassByIdAsync(Guid id)
        {
            var classOffering = await _context.ClassOfferings.FindAsync(id);
            if (classOffering == null) return null;

            return new ClassOfferingDto(classOffering.Id, classOffering.ClassName, classOffering.TotalSeats, classOffering.AvailableSeats);
        }
    }
}
