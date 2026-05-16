namespace SmartCareerPath.Application.DTOs.Auth
{
    public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    string UserId,
    string Email,
    string Role
);

}
