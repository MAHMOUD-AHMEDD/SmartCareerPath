using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.DTOs.Auth;
using SmartCareerPath.Application.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register/seeker")]
    public async Task<IActionResult> RegisterSeeker([FromBody] RegisterSeekerDto dto)
    {
        var result = await _authService.RegisterSeekerAsync(dto);
        return Ok(result);
    }

    [HttpPost("register/mentor")]
    public async Task<IActionResult> RegisterMentor([FromBody] RegisterMentorDto dto)
    {
        var result = await _authService.RegisterMentorAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await _authService.RevokeTokenAsync(userId);
        return NoContent();
    }
}