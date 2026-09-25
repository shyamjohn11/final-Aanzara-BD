using ECommercePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Infrastructure.Services.Email;

/// <summary>
/// IEmailService that only enqueues. Request handlers await this and return
/// immediately; SMTP latency never blocks the HTTP response.
/// </summary>
public sealed class OutboxEmailService : IEmailService
{
    private readonly EmailOutboxQueue _queue;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OutboxEmailService> _logger;

    public OutboxEmailService(
        EmailOutboxQueue queue,
        TimeProvider timeProvider,
        ILogger<OutboxEmailService> logger)
    {
        _queue = queue;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return Task.CompletedTask;
        }

        _queue.Enqueue(new EmailOutboxItem(
            recipientEmail.Trim(),
            subject,
            htmlBody,
            _timeProvider.GetUtcNow()));

        _logger.LogInformation(
            "Email '{Subject}' queued for {Email} (queue depth {Depth}).",
            subject, recipientEmail, _queue.Count);

        return Task.CompletedTask;
    }
}
