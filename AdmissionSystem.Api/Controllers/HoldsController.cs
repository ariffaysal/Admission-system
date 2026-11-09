using Microsoft.AspNetCore.Mvc;
using AdmissionSystem.Api.Interfaces;
using AdmissionSystem.Api.DTOs;
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
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                return Ok(await _admissionService.GetHoldByIdAsync(id));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, [FromHeader(Name = "X-Student-Id")] string studentId)
        {
            if (string.IsNullOrEmpty(studentId)) return BadRequest("X-Student-Id header is missing.");

            try
            {
                await _admissionService.ConfirmHoldAsync(id, studentId);
                return NoContent();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromHeader(Name = "X-Student-Id")] string studentId)
        {
            if (string.IsNullOrEmpty(studentId)) return BadRequest("X-Student-Id header is missing.");

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
