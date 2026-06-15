using SmartCareerPath.Domain.Entites.Identity;
using SmartCareerPath.Domain.Enums;

namespace SmartCareerPath.Domain.Entites
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;  // recipient
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        // Points to ChatRequest.Id or Chat.Id depending on Type — frontend uses this to navigate
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
    }
}
