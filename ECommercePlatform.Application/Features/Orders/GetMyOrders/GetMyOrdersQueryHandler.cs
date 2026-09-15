using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders.GetMyOrders;

public sealed class GetMyOrdersQueryHandler(IOrderRepository orders)
    : IQueryHandler<GetMyOrdersQuery, Result<PagedResult<OrderSummaryResponse>>>
{
    public async Task<Result<PagedResult<OrderSummaryResponse>>> Handle(
        GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var (found, total) = await orders.GetByUserIdAsync(request.UserId, page, pageSize, cancellationToken);

        return Result.Success(new PagedResult<OrderSummaryResponse>(
            found.Select(OrderMappings.ToSummary).ToList(), page, pageSize, total));
    }
}
