using Microsoft.AspNetCore.Identity;

namespace SmartCareerPath.Domain.Entites.Identity
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public RefreshToken? RefreshToken { get; set; }
    }
}
