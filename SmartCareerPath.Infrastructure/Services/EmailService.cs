using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartCareerPath.Application;
using SmartCareerPath.Application.Interfaces;
namespace SmartCareerPath.Infrastructure.Services
{



    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendConfirmationEmailAsync(
            string toEmail, string toName, string confirmationUrl)
        {
            var message = BuildMessage(toEmail, toName,
                "Confirm your Smart Career Path account");

            message.Body = new TextPart("html")
            {
                Text = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;">
                    <h2 style="color:#4F46E5;">Welcome to Smart Career Path!</h2>
                    <p>Hi {toName},</p>
                    <p>Confirm your email by clicking the button below.</p>
                    <div style="text-align:center;margin:32px 0;">
                        <a href="{confirmationUrl}"
                           style="background:#4F46E5;color:white;padding:14px 28px;
                                  border-radius:6px;text-decoration:none;font-weight:bold;">
                            Confirm Email
                        </a>
                    </div>
                    <p style="color:#6B7280;font-size:13px;">This link expires in 24 hours.</p>
                    <p style="color:#6B7280;font-size:13px;">
                        If you did not create an account, ignore this email.
                    </p>
                </div>
                """
            };
            await SendAsync(message);
        }

        public async Task SendPasswordResetEmailAsync(
            string toEmail, string toName, string resetToken)
        {
            var message = BuildMessage(toEmail, toName,
                "Reset your Smart Career Path password");

            // Why no {{ }} or &nbsp; here:
            // Mixing {{ }} brace escaping with HTML entities like &nbsp; inside $""" raw strings
            // causes corruption in Notion and confuses developers copying the code.
            // A clean HTML table shows the same information with zero escaping issues.
            message.Body = new TextPart("html")
            {
                Text = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;">
                    <h2 style="color:#4F46E5;">Password Reset Request</h2>
                    <p>Hi {toName},</p>
                    <p>Copy the token below, then call the endpoint shown to reset your password.</p>

                    <p><strong>Your reset token:</strong></p>
                    <div style="background:#F3F4F6;padding:14px;border-radius:6px;
                                word-break:break-all;font-family:monospace;font-size:12px;">
                        {resetToken}
                    </div>

                    <p style="margin-top:20px;">
                        <strong>Endpoint:</strong> POST {_settings.BaseUrl}/api/auth/reset-password
                    </p>

                    <p><strong>Request body:</strong></p>
                    <table style="border-collapse:collapse;width:100%;font-size:13px;">
                        <tr>
                            <td style="padding:8px;border:1px solid #e5e7eb;"><strong>email</strong></td>
                            <td style="padding:8px;border:1px solid #e5e7eb;">{toEmail}</td>
                        </tr>
                        <tr>
                            <td style="padding:8px;border:1px solid #e5e7eb;"><strong>token</strong></td>
                            <td style="padding:8px;border:1px solid #e5e7eb;">paste the token above</td>
                        </tr>
                        <tr>
                            <td style="padding:8px;border:1px solid #e5e7eb;"><strong>newPassword</strong></td>
                            <td style="padding:8px;border:1px solid #e5e7eb;">YourNewPassword@123</td>
                        </tr>
                        <tr>
                            <td style="padding:8px;border:1px solid #e5e7eb;"><strong>confirmPassword</strong></td>
                            <td style="padding:8px;border:1px solid #e5e7eb;">YourNewPassword@123</td>
                        </tr>
                    </table>

                    <p style="color:#6B7280;font-size:13px;margin-top:20px;">Token expires in 24 hours.</p>
                    <p style="color:#6B7280;font-size:13px;">
                        If you did not request a password reset, ignore this email.
                    </p>
                </div>
                """
            };
            await SendAsync(message);
        }

        private MimeMessage BuildMessage(string toEmail, string toName, string subject)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            msg.To.Add(new MailboxAddress(toName, toEmail));
            msg.Subject = subject;
            return msg;
        }

        private async Task SendAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
                    SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}

