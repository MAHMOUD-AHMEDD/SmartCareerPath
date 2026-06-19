namespace SmartCareerPath.Application.DTOs.Auth
{
    public record ResetPasswordDto(
     string Email,
     string Code,
     string NewPassword,
     string ConfirmPassword);
}
