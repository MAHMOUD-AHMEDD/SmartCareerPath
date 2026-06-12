using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SmartCareerPath.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService) => _chatService = chatService;

        private string UserId =>
            Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        // ASP.NET Core maps the "role" claim back to ClaimTypes.Role correctly when
        // MapInboundClaims is not disabled (default). The fallback to "role" covers any
        // edge-case environment where the mapping differs. Without this, a null role crashes
        // SaveMessageAsync with a NullReferenceException before reaching the service.
        private string UserRole =>
            Context.User!.FindFirst(ClaimTypes.Role)?.Value
            ?? Context.User!.FindFirst("role")?.Value
            ?? throw new InvalidOperationException("Role claim missing from token.");

        public async Task JoinChat(int chatId)
        {
            try
            {
                if (!await _chatService.UserBelongsToChatAsync(UserId, chatId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not a participant of this chat.");
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
                await Clients.Caller.SendAsync("JoinedChat", chatId);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("Error", "Could not join chat.");
            }
        }

        public async Task SendMessage(int chatId, string content)
        {
            // Fix: entire method body is inside try/catch.
            // UserBelongsToChatAsync and the IsNullOrWhiteSpace check are both inside the try block
            // so that any DB unavailability on the authorization check also sends an Error event
            // to the caller instead of silently dropping the WebSocket connection.
            try
            {
                if (!await _chatService.UserBelongsToChatAsync(UserId, chatId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not a participant of this chat.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    await Clients.Caller.SendAsync("Error", "Message content cannot be empty.");
                    return;
                }

                var message = await _chatService.SaveMessageAsync(chatId, UserId, UserRole, content);
                await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", message);
            }
            catch (ArgumentException ex)
            {
                // Content validation failure (e.g. > 2000 chars)
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                // Chat or user not found
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (Exception)
            {
                // DB error or anything unexpected — don't leak internals
                await Clients.Caller.SendAsync("Error", "Failed to send message.");
            }
        }

        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // SignalR removes the connection from all groups automatically on disconnect
            await base.OnDisconnectedAsync(exception);
        }
    }
}
