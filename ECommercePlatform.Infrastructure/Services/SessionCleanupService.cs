using ECommercePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Infrastructure.Services;

/// <summary>
/// Deletes session rows long past their usefulness. Revoked and expired rows are
/// kept for a grace period first so replay detection and audit queries still have
/// something to look at.
/// </summary>
public sealed class SessionCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan RetentionAfterExpiry = TimeSpan.FromDays(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SessionCleanupService> _logger;

    public SessionCleanupService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<SessionCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, _timeProvider);

        // WaitForNextTickAsync throws OperationCanceledException when the host
        // shuts down mid-wait; that is a normal stop, not a failure.
        try
        {
            // Run once at start-up, then on the timer.
            do
            {
                try
                {
                    await PurgeAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // A failed sweep must not take the host down; try again next tick.
                    _logger.LogError(ex, "Session cleanup sweep failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down; exit quietly.
        }
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        var cutoff = _timeProvider.GetUtcNow() - RetentionAfterExpiry;

        using var scope = _scopeFactory.CreateScope();
        var sessions = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

        var deleted = await sessions.PurgeExpiredAsync(cutoff, cancellationToken);

        if (deleted > 0)
        {
            _logger.LogInformation("Session cleanup removed {Count} expired session row(s).", deleted);
        }
    }
}
