namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// Dispatches transactional emails (e.g., OTPs, notifications) to users.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
