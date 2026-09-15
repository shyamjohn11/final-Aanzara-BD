using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Application.Common.Abstractions;
namespace ECommercePlatform.Application.Features.Orders.GetMyOrders;

public sealed record GetMyOrdersQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IQuery<Result<PagedResult<OrderSummaryResponse>>>;
