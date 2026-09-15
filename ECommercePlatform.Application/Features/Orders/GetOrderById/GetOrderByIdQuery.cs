using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders.GetOrderById;

public sealed record GetOrderByIdQuery(Guid UserId, Guid OrderId)
    : IQuery<Result<OrderDetailResponse>>;
