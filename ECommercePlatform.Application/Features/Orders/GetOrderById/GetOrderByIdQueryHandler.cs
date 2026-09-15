using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orders)
    : IQueryHandler<GetOrderByIdQuery, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);

        // A foreign order id is indistinguishable from a missing one, so the
        // caller can never probe for other users' orders.
        if (order is null || order.UserId != request.UserId)
        {
            return Result.Failure<OrderDetailResponse>(OrderErrors.NotFound);
        }

        return Result.Success(OrderMappings.ToDetail(order));
    }
}
