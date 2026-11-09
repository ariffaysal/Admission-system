using Microsoft.AspNetCore.Mvc;
using AdmissionSystem.Api.Interfaces;
using AdmissionSystem.Api.DTOs;
using AdmissionSystem.Api.Services;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AdmissionSystem.Api.Controllers
{
    [ApiController]
    [Route("classes")]
    public class AdmissionController : ControllerBase
    {
        private readonly IAdmissionService _admissionService;

        public AdmissionController(IAdmissionService admissionService)
        {
            _admissionService = admissionService;
        }

        [HttpPost("{classId}/holds")]
        public async Task<IActionResult> CreateHold(Guid classId, [FromHeader(Name = "X-Student-Id")] string studentId)
        {
            if (!StudentId.IsValid(studentId)) return BadRequest(new { error = "Valid X-Student-Id header is required (5–50 digits)." });

            try
            {
                var result = await _admissionService.CreateHoldAsync(classId, studentId);
                return Created($"/holds/{result.HoldId}", result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (SeatUnavailableException ex) { return Conflict(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
        }
    }
}
