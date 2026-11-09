using Microsoft.AspNetCore.Mvc;
using AdmissionSystem.Api.Interfaces;
using AdmissionSystem.Api.DTOs;
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
            if (string.IsNullOrEmpty(studentId)) return BadRequest("X-Student-Id header is missing.");

            try
            {
                var result = await _admissionService.CreateHoldAsync(classId, studentId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
