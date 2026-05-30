using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.Admin;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/lookup-types")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminLookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;
        public AdminLookupController(ILookupService lookupService)
            => _lookupService = lookupService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _lookupService.GetAllTypesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(await _lookupService.GetTypeByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLookupTypeDto dto)
        {
            var result = await _lookupService.CreateTypeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLookupTypeDto dto)
            => Ok(await _lookupService.UpdateTypeAsync(id, dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _lookupService.DeleteTypeAsync(id);
            return NoContent();
        }
    }
}
