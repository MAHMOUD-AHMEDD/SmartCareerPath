using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs;
using SmartCareerPath.Application.DTOs.Question;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers.Admin;

[ApiController]
[Route("api/admin/questions")]
[Authorize(Roles = Roles.Admin)]
public class AdminQuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public AdminQuestionsController(IQuestionService questionService)
        => _questionService = questionService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _questionService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionDto dto)
    {
        var result = await _questionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _questionService.GetByIdAsync(id));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuestionDto dto)
        => Ok(await _questionService.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _questionService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/options")]
    public async Task<IActionResult> AddOption(int id, [FromBody] AddOptionRequest request)
    {
        await _questionService.AddOptionAsync(id, request.OptionText);
        return NoContent();
    }

    [HttpDelete("options/{optionId}")]
    public async Task<IActionResult> DeleteOption(int optionId)
    {
        await _questionService.DeleteOptionAsync(optionId);
        return NoContent();
    }
}

public record AddOptionRequest(string OptionText);