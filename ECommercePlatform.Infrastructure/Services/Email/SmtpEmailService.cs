using System.Net;
using System.Net.Mail;
using ECommercePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommercePlatform.Infrastructure.Services.Email;

/// <summary>
/// Sends transactional emails via SMTP. When SMTP credentials are not configured
/// in development, it logs the email content for offline local testing.
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailOptions> options,
        IHostEnvironment environment,
        ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        // When SMTP credentials are unconfigured, log the message so developers and testers
        // can retrieve OTP codes straight from the console/logs.
        if (_environment.IsDevelopment() && string.IsNullOrWhiteSpace(_options.Username))
        {
            _logger.LogInformation(
                """
                ================== TRANSACTIONAL EMAIL (DEV MOCK) ==================
                To: {Recipient}
                From: {Sender} <{SenderEmail}>
                Subject: {Subject}
                Body:
                {Body}
                ====================================================================
                """,
                recipientEmail,
                _options.SenderName,
                _options.SenderEmail,
                subject,
                htmlBody);
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.SenderEmail, _options.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(recipientEmail));

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.AppPassword))
            {
                // Gmail app passwords are often pasted with spaces (xbnh anck ...); strip them for SMTP auth.
                var cleanPassword = _options.AppPassword.Replace(" ", string.Empty).Trim();
                client.Credentials = new NetworkCredential(_options.Username.Trim(), cleanPassword);
            }

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully to {Recipient} via SMTP ({Host}:{Port}).", recipientEmail, _options.Host, _options.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} via SMTP ({Host}:{Port}).", recipientEmail, _options.Host, _options.Port);

            // In Development the OTP is time-sensitive (10 min, single-use). If SMTP is
            // unreachable (firewall, wrong app password), still surface the rendered
            // email in logs so testers can retrieve the code without blocking the
            // password-reset flow — then rethrow in Production so the caller sees 500.
            if (_environment.IsDevelopment())
            {
                _logger.LogWarning(
                    """
                    ================== TRANSACTIONAL EMAIL (FALLBACK LOG — SMTP FAILED) ==================
                    To: {Recipient}
                    From: {Sender} <{SenderEmail}>
                    Subject: {Subject}
                    Body:
                    {Body}
                    ========================================================================================
                    """,
                    recipientEmail,
                    _options.SenderName,
                    _options.SenderEmail,
                    subject,
                    htmlBody);
                return;
            }

            throw;
        }
    }
}
