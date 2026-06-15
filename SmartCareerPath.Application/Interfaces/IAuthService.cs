using SmartCareerPath.Application.DTOs.Auth;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterSeekerAsync(RegisterSeekerDto dto);
        Task<AuthResponseDto> RegisterMentorAsync(RegisterMentorDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string userId);
        Task LogoutAsync(string userId);
        // Email confirmation
        Task ConfirmEmailAsync(string email, string token);
        Task ResendConfirmationEmailAsync(string email);

        // Password reset
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}
