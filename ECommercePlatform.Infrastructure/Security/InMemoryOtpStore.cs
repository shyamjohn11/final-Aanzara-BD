using System.Collections.Concurrent;
using ECommercePlatform.Application.Common.Abstractions;

namespace ECommercePlatform.Infrastructure.Security;

/// <summary>
/// Thread-safe in-memory store for one-time passwords (OTPs).
/// Enforces expiration, attempt limits to prevent brute-force attacks,
/// and atomic single-use consumption.
/// </summary>
public sealed class InMemoryOtpStore : IOtpStore
{
    private const int MaxFailedAttempts = 3;

    private readonly ConcurrentDictionary<string, OtpEntry> _store = new();
    private readonly TimeProvider _timeProvider;

    public InMemoryOtpStore(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task SetOtpAsync(
        string email,
        string purpose,
        string otp,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(email, purpose);
        var now = _timeProvider.GetUtcNow();

        _store[key] = new OtpEntry(otp, now.Add(expiry), MaxFailedAttempts);

        PruneExpired(now);

        return Task.CompletedTask;
    }

    public Task<bool> ValidateAndConsumeOtpAsync(
        string email,
        string purpose,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(email, purpose);
        var now = _timeProvider.GetUtcNow();

        if (!_store.TryGetValue(key, out var entry))
        {
            return Task.FromResult(false);
        }

        if (now > entry.ExpiresAt)
        {
            _store.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        if (entry.AttemptsRemaining <= 0)
        {
            _store.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        // Constant-time string comparison or ordinal string match
        if (string.Equals(entry.Otp, otp, StringComparison.Ordinal))
        {
            // Atomically consume so it cannot be reused.
            _store.TryRemove(key, out _);
            return Task.FromResult(true);
        }

        // Decrement remaining attempts on mismatch to prevent brute forcing.
        var updated = entry with { AttemptsRemaining = entry.AttemptsRemaining - 1 };
        if (updated.AttemptsRemaining <= 0)
        {
            _store.TryRemove(key, out _);
        }
        else
        {
            _store.TryUpdate(key, updated, entry);
        }

        return Task.FromResult(false);
    }

    private static string BuildKey(string email, string purpose)
        => $"{email.Trim().ToUpperInvariant()}:{purpose.Trim().ToUpperInvariant()}";

    private void PruneExpired(DateTimeOffset now)
    {
        if (_store.Count < 50)
        {
            return;
        }

        foreach (var (key, entry) in _store)
        {
            if (now > entry.ExpiresAt)
            {
                _store.TryRemove(key, out _);
            }
        }
    }

    private sealed record OtpEntry(string Otp, DateTimeOffset ExpiresAt, int AttemptsRemaining);
}
