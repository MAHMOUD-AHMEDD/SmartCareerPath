using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Interfaces;
using System.Security.Claims;

namespace SmartCareerPath.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
            => _service = service;

        private string UserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        // GET /api/notifications?page=1&pageSize=10
        // Called when user opens the notification bell dropdown — no separate page needed
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
            => Ok(await _service.GetMyNotificationsAsync(UserId, page, pageSize));

        // GET /api/notifications/unread-count
        // Called on every page load — drives the badge number on the bell icon
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
            => Ok(new { count = await _service.GetUnreadCountAsync(UserId) });

        // PUT /api/notifications/{id}/read — mark one as read (on click)
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsReadAsync(id, UserId);
            return NoContent();
        }

        // PUT /api/notifications/read-all — mark all as read
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _service.MarkAllAsReadAsync(UserId);
            return NoContent();
        }
    }
}
