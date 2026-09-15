using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Notifications;

// IDs #139-141 — list / PATCH /{id}/read / DELETE.
// Persisted in SQL Server (Notifications table). Wire shape unchanged.

public sealed record NotificationResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; }
    public bool IsRead { get; init; }
    public string Type { get; init; } = "system";
    public string? Link { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetNotificationsQuery : IQuery<Result<PagedResult<NotificationResponse>>>
{
    public string? Search { get; init; }
    public bool? UnreadOnly { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record MarkNotificationReadCommand(Guid Id, bool IsRead = true)
    : ICommand<Result<NotificationResponse>>;

public sealed record DeleteNotificationCommand(Guid Id) : ICommand<Result>;

internal static class NotificationMappings
{
    internal static NotificationResponse ToDto(this Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Message = notification.Message,
        IsRead = notification.IsRead,
        Type = notification.Type,
        Link = notification.Link,
        CreatedAt = notification.CreatedAt
    };
}

public sealed class GetNotificationsQueryHandler
    : IQueryHandler<GetNotificationsQuery, Result<PagedResult<NotificationResponse>>>
{
    private readonly IAdminRepository<Notification> _notifications;

    public GetNotificationsQueryHandler(IAdminRepository<Notification> notifications)
        => _notifications = notifications;

    public async Task<Result<PagedResult<NotificationResponse>>> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<Notification, bool>> filter = AdminFilters.True<Notification>();

        if (request.UnreadOnly == true)
        {
            filter = filter.And(n => !n.IsRead);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(n =>
                n.Title.Contains(s)
                || (n.Message != null && n.Message.Contains(s)));
        }

        var total = await _notifications.CountAsync(filter, cancellationToken);
        var items = await _notifications.PageAsync(
            filter,
            q => q.OrderByDescending(n => n.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<NotificationResponse>(
            items.Select(n => n.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, Result<NotificationResponse>>
{
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(
        IAdminRepository<Notification> notifications, IUnitOfWork unitOfWork)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NotificationResponse>> Handle(
        MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.Id, cancellationToken);

        if (notification is null)
        {
            return Result.Failure<NotificationResponse>(AdminErrors.NotFound("Notification", request.Id));
        }

        notification.IsRead = request.IsRead;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(notification.ToDto());
    }
}

public sealed class DeleteNotificationCommandHandler
    : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationCommandHandler(
        IAdminRepository<Notification> notifications, IUnitOfWork unitOfWork)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.Id, cancellationToken);

        if (notification is null)
        {
            return Result.Failure(AdminErrors.NotFound("Notification", request.Id));
        }

        _notifications.Remove(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
