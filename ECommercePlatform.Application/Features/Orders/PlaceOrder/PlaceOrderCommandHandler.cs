using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Addresses;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Application.Features.Cart;
using Cart = ECommercePlatform.Domain.Entities.Cart;
using ECommercePlatform.Domain.Enums;

using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace ECommercePlatform.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orders,
    ICartRepository carts,
    IAddressRepository addresses,
    IUserRepository users,
    IInventoryRepository inventory,
    ITaxRuleRepository taxRules,
    IDeliveryRuleRepository deliveryRules,
    IAdminRepository<Notification> notifications,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEmailService emailService,
    ILogger<PlaceOrderCommandHandler> logger)
    : ICommandHandler<PlaceOrderCommand, Result<PlaceOrderResponse>>
{
    public async Task<Result<PlaceOrderResponse>> Handle(
        PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<PlaceOrderResponse>(OrderErrors.NotAuthenticated);
        }

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var method))
        {
            return Result.Failure<PlaceOrderResponse>(OrderErrors.InvalidPaymentMethod);
        }

        var address = await addresses.GetByIdAndUserIdAsync(request.AddressId, request.UserId, cancellationToken);
        if (address is null)
        {
            return Result.Failure<PlaceOrderResponse>(AddressErrors.NotFound);
        }

        var cart = await carts.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null || cart.CartItems.Count == 0)
        {
            return Result.Failure<PlaceOrderResponse>(OrderErrors.EmptyCart);
        }

        var stockErrors = await CheckStockAsync(cart, cancellationToken);
        if (stockErrors.Length > 0)
        {
            return Result.Failure<PlaceOrderResponse>(
                OrderErrors.InsufficientStock(string.Join(", ", stockErrors)));
        }

        // Reuse the cart pricing engine so the order's totals can never drift
        // from what the cart page displayed at the moment of checkout.
        var now = timeProvider.GetUtcNow();
        var summary = CartPricing.Summarize(
            cart.CartItems.Select(i => new CartLine(i.Product, i.Quantity)).ToArray(),
            await taxRules.GetAllAsync(cancellationToken),
            await deliveryRules.GetAsync(cancellationToken),
            cart.AppliedCoupon,
            now);

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = request.UserId,
            OrderStatus = OrderStatus.Pending,
            ItemsTotal = summary.Subtotal,
            Discount = summary.DiscountTotal,
            GstAmount = summary.TaxTotal,
            DeliveryCharge = summary.DeliveryCharge,
            HandlingFee = summary.HandlingFee,
            GrandTotal = summary.Total,
            // Only snapshot the coupon when it actually discounted something;
            // an expired coupon stays null on the order.
            AppliedCouponId = summary.AppliedCouponCode is null ? null : cart.AppliedCouponId,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var item in cart.CartItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            });
        }

        orders.Add(order);

        orders.AddAddress(new OrderAddress
        {
            OrderAddressId = Guid.NewGuid(),
            OrderId = order.OrderId,
            RecipientName = user.Name,
            RecipientPhone = user.Phone ?? string.Empty,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            Pincode = address.Pincode,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            CreatedAt = now
        });

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = order.OrderId,
            UserId = request.UserId,
            PaymentMethod = method,
            // COD collects at the door, so there is no gateway behind it.
            PaymentGateway = method == PaymentMethod.COD ? null : "Manual",
            Amount = order.GrandTotal,
            Currency = "INR",
            Status = method == PaymentMethod.COD ? PaymentStatus.Pending : PaymentStatus.Initiated,
            CreatedAt = now,
            UpdatedAt = now
        };
        orders.AddPayment(payment);

        orders.AddStatusHistory(new OrderStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Status = OrderStatus.Pending,
            ChangedByUserId = request.UserId,
            Remarks = "Order placed.",
            ChangedAt = now.UtcDateTime
        });

        // Free the items for re-purchase; the cart row itself stays for reuse.
        foreach (var item in cart.CartItems)
        {
            carts.RemoveItem(item);
        }

        var orderNo = OrderMappings.OrderNoFor(order);
        NotificationEmitter.Emit(
            notifications,
            "order",
            $"New order {orderNo}",
            $"{user.Name} placed an order of ₹{order.GrandTotal} ({cart.CartItems.Count} items).",
            "/admin/orders");

        // Snapshot the cart lines for the confirmation email before SaveChanges:
        // order items only carry ProductId, while the email needs names, and
        // the in-memory cart objects still hold their Product navs here.
        var emailLines = cart.CartItems
            .Select(i => new OrderEmailLine(i.Product.ProductName, i.Quantity, i.Product.Price))
            .ToList();
        var shipTo = string.IsNullOrWhiteSpace(user.Phone)
            ? user.Name
            : $"{user.Name} ({user.Phone.Trim()})";
        var addressSummary = string.Join(", ", new[]
            {
                address.AddressLine1,
                address.AddressLine2,
                address.City,
                address.State,
                address.Pincode
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

        // One save for the order, items, address snapshot, payment, history,
        // and the emptied cart — checkout either fully happens or not at all.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort user email + already-saved admin notification row above
        // complete the order-placed flow. Email failure must never fail checkout.
        await OrderEmailSender.TrySendAsync(
            emailService,
            logger,
            user.Email,
            $"Order {orderNo} confirmed",
            OrderEmailSender.PlaceOrderHtml(
                user.Name,
                orderNo,
                emailLines,
                summary.Subtotal,
                summary.DiscountTotal,
                summary.TaxTotal,
                summary.DeliveryCharge,
                summary.HandlingFee,
                summary.Total,
                summary.AppliedCouponCode,
                payment.PaymentMethod.ToString(),
                payment.Status.ToString(),
                shipTo,
                addressSummary),
            cancellationToken);

        return Result.Success(new PlaceOrderResponse
        {
            OrderId = order.OrderId,
            OrderNo = orderNo,
            Status = order.OrderStatus.ToString(),
            GrandTotal = order.GrandTotal,
            PaymentId = payment.PaymentId,
            PaymentStatus = payment.Status.ToString(),
            PaymentMethod = payment.PaymentMethod.ToString()
        });
    }

    private async Task<string[]> CheckStockAsync(ECommercePlatform.Domain.Entities.Cart cart, CancellationToken cancellationToken)
    {
        var productIds = cart.CartItems.Select(i => i.ProductId).Distinct().ToArray();

        var availableByProduct = (await inventory.GetByProductIdsAsync(productIds, cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.StockQuantity - i.ReservedQuantity));

        // Products with no inventory rows are sellable without a cap (same
        // rule as the shelf and the cart): only tracked stock can block
        // checkout, so a missing key means "no limit", not "zero".
        return cart.CartItems
            .Where(i => availableByProduct.TryGetValue(i.ProductId, out var available) && i.Quantity > available)
            .Select(i => i.Product.ProductName)
            .ToArray();
    }
}
