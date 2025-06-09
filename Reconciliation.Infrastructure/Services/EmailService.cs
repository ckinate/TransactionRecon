using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reconciliation.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string email, string verificationLink)
        {
            try
            {
                using var client = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
                {
                    Port = int.Parse(_configuration["EmailSettings:Port"]),
                    Credentials = new NetworkCredential(
                        _configuration["EmailSettings:Username"],
                        _configuration["EmailSettings:Password"]),
                    EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSSL"]),
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["EmailSettings:FromEmail"], "Bank Reconciliation"),
                    Subject = "Verify Your Email Address",
                    Body = $@"
                    <html>
                    <body>
                        <h2>Welcome to Our Application!</h2>
                        <p>Please click the link below to verify your email address and activate your account:</p>
                        <p><a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-decoration: none; border-radius: 4px;'>Verify Email</a></p>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p>{verificationLink}</p>
                        <p>This link will expire in 24 hours.</p>
                    </body>
                    </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                await client.SendMailAsync(mailMessage);

                _logger.LogInformation($"Verification email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send verification email to {email}");
                throw;
            }

        }
    }
}
