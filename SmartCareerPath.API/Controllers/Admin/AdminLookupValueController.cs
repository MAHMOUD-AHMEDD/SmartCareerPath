using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.Admin;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/lookup-types/{typeId}/values")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminLookupValueController : ControllerBase
    {
        private readonly ILookupService _lookupService;
        public AdminLookupValueController(ILookupService lookupService)
            => _lookupService = lookupService;

        [HttpGet]
        public async Task<IActionResult> GetValues(int typeId)
            => Ok(await _lookupService.GetValuesByTypeAsync(typeId));

        [HttpPost]
        public async Task<IActionResult> Create(int typeId, [FromBody] CreateLookupValueDto dto)
        {
            var createDto = dto with { LookupTypeId = typeId };
            var result = await _lookupService.CreateValueAsync(createDto);
            return CreatedAtAction(nameof(GetValues), new { typeId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLookupValueDto dto)
            => Ok(await _lookupService.UpdateValueAsync(id, dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _lookupService.DeleteValueAsync(id);
            return NoContent();
        }
    }
}
