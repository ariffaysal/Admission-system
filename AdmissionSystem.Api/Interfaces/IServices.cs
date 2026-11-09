using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdmissionSystem.Api.DTOs;

namespace AdmissionSystem.Api.Interfaces
{
    public interface IClassService
    {
        Task<ClassOfferingDto> CreateClassAsync(CreateClassRequest request);
        Task<IEnumerable<ClassOfferingDto>> GetAllClassesAsync();
        Task<ClassOfferingDto?> GetClassByIdAsync(Guid id);
    }

    public interface IAdmissionService
    {
        Task<AdmissionHoldDto> CreateHoldAsync(Guid classId, string studentId);
        Task<AdmissionHoldDto> GetHoldByIdAsync(Guid holdId, string studentId);
        Task<AdmissionHoldDto> ConfirmHoldAsync(Guid holdId, string studentId);
        Task CancelHoldAsync(Guid holdId, string studentId);
        Task<IEnumerable<AdmissionHoldDto>> GetHoldsForStudentAsync(string studentId);
        Task ExpireAllAsync();
    }
}
