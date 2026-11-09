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
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassesController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
        {
            try
            {
                var result = await _classService.CreateClassAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _classService.GetAllClassesAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _classService.GetClassByIdAsync(id);
            if (result == null) return NotFound(new { error = "Class not found." });
            return Ok(result);
        }
    }
}
