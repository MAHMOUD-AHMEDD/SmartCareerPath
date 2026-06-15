namespace SmartCareerPath.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationUrl);
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken);
    }
}
