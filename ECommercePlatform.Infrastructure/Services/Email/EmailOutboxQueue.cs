using System.Collections.Concurrent;

namespace ECommercePlatform.Infrastructure.Services.Email;

/// <summary>
/// Process-wide FIFO of outbound emails. Handlers enqueue after SaveChanges;
/// the BackgroundService drains and sends off the request thread.
/// </summary>
public sealed class EmailOutboxQueue
{
    private readonly ConcurrentQueue<EmailOutboxItem> _queue = new();

    public int Count => _queue.Count;

    public void Enqueue(EmailOutboxItem item) => _queue.Enqueue(item);

    public bool TryDequeue(out EmailOutboxItem item)
        => _queue.TryDequeue(out item!);
}

public sealed record EmailOutboxItem(
    string Recipient,
    string Subject,
    string HtmlBody,
    DateTimeOffset EnqueuedAt);
