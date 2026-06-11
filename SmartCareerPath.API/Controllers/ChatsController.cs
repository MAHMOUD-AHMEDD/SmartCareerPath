using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.Chat;
using SmartCareerPath.Application.Interfaces;
using System.Security.Claims;

namespace SmartCareerPath.API.Controllers
{
    [ApiController]
    [Route("api/chats")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService) => _chatService = chatService;

        private string UserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        // Same null-safe pattern as ChatHub — FindFirst returns null if the claim is absent.
        // Using !.Value without a null check throws NullReferenceException → 500 instead of
        // a meaningful error. The "role" fallback covers environments where the JWT handler
        // does not map the short claim name back to ClaimTypes.Role.
        private string UserRole =>
            User.FindFirst(ClaimTypes.Role)?.Value
            ?? User.FindFirst("role")?.Value
            ?? throw new InvalidOperationException("Role claim missing from token.");

        // POST /api/chats — Seeker only
        [HttpPost]
        [Authorize(Roles = Roles.Seeker)]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
        {
            var result = await _chatService.CreateChatAsync(UserId, dto);
            return CreatedAtAction(nameof(GetHistory), new { id = result.Id }, result);
        }

        // GET /api/chats?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetMyChats(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
            => Ok(await _chatService.GetUserChatsAsync(UserId, UserRole, page, pageSize));

        // GET /api/chats/{id}/messages?page=1&pageSize=20
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetHistory(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Authorization: only participants can read history
            if (!await _chatService.UserBelongsToChatAsync(UserId, id))
                return Forbid();

            return Ok(await _chatService.GetChatHistoryAsync(id, UserId, page, pageSize));
        }
    }
}
