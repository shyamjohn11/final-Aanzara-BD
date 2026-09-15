using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid UserId, Guid OrderId, string? Reason)
    : ICommand<Result<OrderDetailResponse>>;
