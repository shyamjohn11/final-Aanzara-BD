using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Infrastructure.Services.Email;

/// <summary>
/// Drains <see cref="EmailOutboxQueue"/> and sends via <see cref="SmtpEmailService"/>.
/// Runs on a short timer so order/OTP emails leave within seconds without
/// blocking any request thread.
/// </summary>
public sealed class EmailOutboxService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EmailOutboxQueue _queue;
    private readonly ILogger<EmailOutboxService> _logger;

    public EmailOutboxService(
        IServiceScopeFactory scopeFactory,
        EmailOutboxQueue queue,
        ILogger<EmailOutboxService> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        try
        {
            do
            {
                try
                {
                    await DrainAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email outbox drain failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutting down.
        }
    }

    private async Task DrainAsync(CancellationToken cancellationToken)
    {
        while (_queue.TryDequeue(out var item))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var smtp = scope.ServiceProvider.GetRequiredService<SmtpEmailService>();
                await smtp.SendEmailAsync(item.Recipient, item.Subject, item.HtmlBody, cancellationToken);
                _logger.LogInformation(
                    "Outbox sent '{Subject}' to {Email}.", item.Subject, item.Recipient);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Requeue is best-effort on shutdown; drop is acceptable.
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Outbox failed to send '{Subject}' to {Email}.",
                    item.Subject, item.Recipient);
            }
        }
    }
}
