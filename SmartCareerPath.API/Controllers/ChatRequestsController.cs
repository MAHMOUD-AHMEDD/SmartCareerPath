using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.ChatRequest;
using SmartCareerPath.Application.Interfaces;
using System.Security.Claims;

namespace SmartCareerPath.API.Controllers
{
    [ApiController]
    [Route("api/chat-requests")]
    [Authorize]
    public class ChatRequestsController : ControllerBase
    {
        private readonly IChatRequestService _service;

        public ChatRequestsController(IChatRequestService service)
            => _service = service;

        private string UserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        // POST /api/chat-requests — Seeker sends a request to a mentor
        [HttpPost]
        [Authorize(Roles = Roles.Seeker)]
        public async Task<IActionResult> SendRequest([FromBody] SendChatRequestDto dto)
        {
            var result = await _service.SendRequestAsync(UserId, dto);
            return CreatedAtAction(nameof(GetOutgoing), result);
        }

        // GET /api/chat-requests/incoming?status=Pending&page=1&pageSize=10
        // Mentor sees all requests sent to them — filter by ?status=Pending for action needed
        [HttpGet("incoming")]
        [Authorize(Roles = Roles.Mentor)]
        public async Task<IActionResult> GetIncoming(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
            => Ok(await _service.GetIncomingAsync(UserId, status, page, pageSize));

        // GET /api/chat-requests/outgoing?status=Pending&page=1&pageSize=10
        // Seeker sees all their requests and current statuses — filter by ?status= optionally
        [HttpGet("outgoing")]
        [Authorize(Roles = Roles.Seeker)]
        public async Task<IActionResult> GetOutgoing(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
            => Ok(await _service.GetOutgoingAsync(UserId, status, page, pageSize));

        // PUT /api/chat-requests/{id}/accept — Mentor accepts
        // Returns chatId so the frontend can navigate directly to the new chat
        [HttpPut("{id}/accept")]
        [Authorize(Roles = Roles.Mentor)]
        public async Task<IActionResult> Accept(int id)
        {
            var chatId = await _service.AcceptAsync(id, UserId);
            return Ok(new { message = "Chat request accepted.", chatId });
        }

        // PUT /api/chat-requests/{id}/decline — Mentor declines
        [HttpPut("{id}/decline")]
        [Authorize(Roles = Roles.Mentor)]
        public async Task<IActionResult> Decline(int id)
        {
            await _service.DeclineAsync(id, UserId);
            return Ok(new { message = "Chat request declined." });
        }
    }
}
