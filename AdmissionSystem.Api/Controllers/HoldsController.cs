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
    [Route("holds")]
    public class HoldsController : ControllerBase
    {
        private readonly IAdmissionService _admissionService;

        public HoldsController(IAdmissionService admissionService)
        {
            _admissionService = admissionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromHeader(Name = "X-Student-Id")] string studentId)
        {
            if (!StudentId.IsValid(studentId)) return BadRequest(new { error = "Valid X-Student-Id header is required (5–50 digits)." });
            try
            {
                return Ok(await _admissionService.GetHoldByIdAsync(id, studentId));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Reservation not found." });
            }
            catch (UnauthorizedAccessException) { return StatusCode(403, new { error = "Student does not own this reservation." }); }
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, [FromHeader(Name = "X-Student-Id")] string studentId)
        {
            if (!StudentId.IsValid(studentId)) return BadRequest(new { error = "Valid X-Student-Id header is required (5–50 digits)." });

            try
            {
                return Ok(await _admissionService.ConfirmHoldAsync(id, studentId));
            }
            catch (KeyNotFoundException) { return NotFound(new { error = "Reservation not found." }); }
            catch (UnauthorizedAccessException) { return StatusCode(403, new { error = "Student does not own this reservation." }); }
            catch (HoldExpiredException ex) { return Conflict(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromHeader(Name = "X-Student-Id")] string studentId)
        {
            if (!StudentId.IsValid(studentId)) return BadRequest(new { error = "Valid X-Student-Id header is required (5–50 digits)." });

            try
            {
                await _admissionService.CancelHoldAsync(id, studentId);
                return NoContent();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
    }
}

public static class StudentId
{
    public static bool IsValid(string? value) => value is { Length: >= 5 and <= 50 } && value.All(c => c is >= '0' and <= '9');
}
