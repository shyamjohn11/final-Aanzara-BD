using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Orders.ConfirmDealerOrder;

public sealed class ConfirmDealerOrderCommandHandler(
    IOrderRepository orders,
    IAdminRepository<Dealer> dealers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ConfirmDealerOrderCommandHandler> logger) : ICommandHandler<ConfirmDealerOrderCommand, Result>
{
    public async Task<Result> Handle(ConfirmDealerOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) return Result.Failure(OrderErrors.NotFound);

        if (order.DealerId != request.DealerId)
        {
            // Ensure this dealer owns the order's dealer shop
            return Result.Failure(Error.Forbidden("dealer.not_owner", "This order does not belong to your shop."));
        }

        var dealer = await dealers.GetByIdAsync(request.DealerId, cancellationToken);
        if (dealer is null) return Result.Failure(Error.NotFound("dealer.not_found", "Dealer shop not found."));
        if (dealer.AgentId != request.AgentId) return Result.Failure(Error.Forbidden("dealer.not_agent", "You are not the agent for this shop."));

        if (order.OrderStatus != OrderStatus.Pending)
        {
            return Result.Failure(Error.Validation("order.invalid_status", $"Order is already {order.OrderStatus}, cannot confirm."));
        }

        var now = timeProvider.GetUtcNow();
        order.OrderStatus = OrderStatus.Confirmed;
        order.CourierName = $"{dealer.ShopName} — Direct Ship";
        order.CurrentLocation = $"{dealer.City}, {dealer.State}";
        order.EstimatedDeliveryDate = now.AddDays(2);
        order.UpdatedAt = now;

        orders.AddStatusHistory(new OrderStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Status = OrderStatus.Confirmed,
            ChangedByUserId = request.AgentId, // agent confirms on behalf of dealer
            Remarks = $"Dealer {dealer.ShopName} confirmed order. Tracking: {order.TrackingNumber}",
            ChangedAt = now.UtcDateTime
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Dealer {DealerId} confirmed order {OrderId}.", request.DealerId, request.OrderId);
        return Result.Success();
    }
}
