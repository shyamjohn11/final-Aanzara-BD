using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Admin.Notifications;

/// <summary>
/// Single place where domain events become admin-feed rows. Handlers call
/// <see cref="Emit"/> before their own SaveChanges so the notification
/// commits in the same transaction as the event that caused it.
/// Types: order, payment, enquiry, pricing, quote, review, inventory,
/// account, system.
/// </summary>
internal static class NotificationEmitter
{
    internal static void Emit(
        IAdminRepository<Notification> notifications,
        string type,
        string title,
        string? message = null,
        string? link = null)
    {
        var now = DateTimeOffset.UtcNow;
        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Type = string.IsNullOrWhiteSpace(type) ? "system" : type.Trim().ToLowerInvariant(),
            Title = title.Trim(),
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
            Link = string.IsNullOrWhiteSpace(link) ? null : link.Trim(),
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now,
        });
    }
}
