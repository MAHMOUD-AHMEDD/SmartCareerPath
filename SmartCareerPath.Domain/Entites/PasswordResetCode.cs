using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class PasswordResetCode
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;  // 6-digit
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public AppUser User { get; set; } = null!;
    }
}
