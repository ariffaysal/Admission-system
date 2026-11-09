using Microsoft.AspNetCore.Mvc;
using AdmissionSystem.Api.Interfaces;
using AdmissionSystem.Api.DTOs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AdmissionSystem.Api.Controllers
{
    [ApiController]
    [Route("me")]
    public class MeController : ControllerBase
    {
        private readonly IAdmissionService _admissionService;

        public MeController(IAdmissionService admissionService)
        {
            _admissionService = admissionService;
        }

        [HttpGet("holds")]
        public async Task<IActionResult> GetMyHolds([FromHeader(Name = "X-Student-Id")] string studentId)
        {
            if (string.IsNullOrEmpty(studentId)) return BadRequest("X-Student-Id header is missing.");

            return Ok(await _admissionService.GetHoldsForStudentAsync(studentId));
        }
    }
}
