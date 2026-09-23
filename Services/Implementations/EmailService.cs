using System.Net;
using System.Net.Mail;
using InventoryManagementSystem.Services.Interfaces;

namespace InventoryManagementSystem.Services.Implementations
{
    public class SmtpSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; } = false;
        public string SenderEmail { get; set; } = "noreply@ims.com";
        public string SenderName { get; set; } = "IMS Notifications";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false; // Dev mode fallback
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _settings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings();
            _logger = logger;
        }

        public async Task SendEmailNotificationAsync(string recipientEmail, string subject, string bodyMessage)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail)) return;

            // Development fallback mode when SMTP is unconfigured or disabled
            if (!_settings.IsEnabled || string.IsNullOrWhiteSpace(_settings.Host) || _settings.Host == "localhost")
            {
                _logger.LogInformation("[DEV EMAIL DISPATCHED] To: {Recipient} | Subject: '{Subject}' | Message: {Body}",
                    recipientEmail, subject, bodyMessage);
                return;
            }

            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
                message.To.Add(new MailAddress(recipientEmail));
                message.Subject = subject;
                message.Body = bodyMessage;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(_settings.Host, _settings.Port);
                client.EnableSsl = _settings.EnableSsl;
                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                }

                await client.SendMailAsync(message);
                _logger.LogInformation("Email successfully sent via SMTP to {Recipient}", recipientEmail);
            }
            catch (Exception ex)
            {
                // Per SRS: Asset CRUD operations must NOT fail solely because email delivery failed.
                _logger.LogError(ex, "Failed to deliver SMTP email to {Recipient}. Error logged, operation continuing.", recipientEmail);
            }
        }
    }
}
