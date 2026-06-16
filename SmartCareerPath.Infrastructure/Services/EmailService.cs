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

            message.Body = new TextPart("html")
            {
                Text = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;">
                    <h2 style="color:#4F46E5;">Reset Your Password</h2>
                    <p>Hi {toName},</p>
                    <p>We received a request to reset your Smart Career Path password.</p>
                    <p>Use the code below to create a new password:</p>

                    <div style="background:#F3F4F6;border:2px dashed #4F46E5;border-radius:8px;
                                padding:20px;text-align:center;margin:24px 0;">
                        <p style="color:#6B7280;font-size:12px;margin:0 0 10px 0;">
                            YOUR RESET CODE
                        </p>
                        <div style="font-family:monospace;font-size:14px;font-weight:bold;
                                    word-break:break-all;color:#1F2937;">
                            {resetToken}
                        </div>
                    </div>

                    <p>
                        Go to the app, open the <strong>Reset Password</strong> page,
                        and enter this code along with your new password.
                    </p>

                    <p style="color:#6B7280;font-size:13px;margin-top:24px;">
                        This code expires in 24 hours.
                    </p>
                    <p style="color:#6B7280;font-size:13px;">
                        If you did not request a password reset, you can safely ignore this email.
                        Your account remains secure.
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


