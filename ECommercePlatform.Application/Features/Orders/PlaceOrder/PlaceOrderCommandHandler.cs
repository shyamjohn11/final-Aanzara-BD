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
    IWarehouseRepository warehouses,
    ITaxRuleRepository taxRules,
    IDeliveryRuleRepository deliveryRules,
    IAdminRepository<Notification> notifications,
    IAdminRepository<ECommercePlatform.Domain.Entities.Dealer> dealers,
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

        // Warehouse-wise: fetch all warehouses and inventories for allocation
        var warehousePage = await warehouses.SearchAsync(null, null, 1, 100, cancellationToken);
        var allWarehouses = warehousePage.Items.ToList();
        var productIds = cart.CartItems.Select(i => i.ProductId).Distinct().ToArray();
        var inventories = await inventory.GetByProductIdsAsync(productIds, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var summary = CartPricing.Summarize(
            cart.CartItems.Select(i => new CartLine(i.Product, i.Quantity)).ToArray(),
            await taxRules.GetAllAsync(cancellationToken),
            await deliveryRules.GetAsync(cancellationToken),
            cart.AppliedCoupon,
            now);

        // Determine fulfillment: dealer shops vs warehouses
        var dealerItems = cart.CartItems.Where(i => i.Product.DealerId.HasValue).ToList();
        var warehouseItems = cart.CartItems.Where(i => !i.Product.DealerId.HasValue).ToList();

        Guid? primaryWarehouseId = null;
        Guid? primaryDealerId = null;
        string? currentLocation = null;
        var trackingNumber = $"TRK{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
        var courier = "Aanzara Logistics";
        var estimatedDelivery = now.AddDays(3);

        if (warehouseItems.Count > 0)
        {
            // For each warehouse product, find nearest warehouse with stock
            foreach (var item in warehouseItems)
            {
                var nearest = ECommercePlatform.Application.Services.WarehouseLocator.FindNearest(
                    allWarehouses,
                    inventories.ToList(),
                    item.ProductId,
                    item.Quantity,
                    address.City ?? string.Empty,
                    address.State ?? string.Empty,
                    address.Latitude == 0 ? null : (double?)address.Latitude,
                    address.Longitude == 0 ? null : (double?)address.Longitude);

                if (nearest is not null)
                {
                    primaryWarehouseId ??= nearest.WarehouseId;
                    currentLocation ??= $"{nearest.City ?? nearest.WarehouseName}, {nearest.State ?? string.Empty}".Trim().TrimEnd(',');

                    // Reserve stock in that warehouse
                    var inv = inventories.FirstOrDefault(i => i.ProductId == item.ProductId && i.WarehouseId == nearest.WarehouseId);
                    if (inv is not null)
                    {
                        inv.ReservedQuantity += item.Quantity;
                    }
                }
            }
            currentLocation ??= primaryWarehouseId.HasValue
                ? allWarehouses.FirstOrDefault(w => w.WarehouseId == primaryWarehouseId.Value)?.City ?? "Warehouse"
                : "Warehouse";
        }

        if (dealerItems.Count > 0)
        {
            primaryDealerId = dealerItems.First().Product.DealerId;
            // Dealer shop location will be used for tracking
            try
            {
                var dealer = await dealers.GetByIdAsync(primaryDealerId.Value, cancellationToken);
                if (dealer is not null)
                {
                    currentLocation = $"{dealer.City ?? dealer.ShopName}, {dealer.State ?? string.Empty}".Trim().TrimEnd(',');
                }
            }
            catch { }
        }

        var orderStatus = dealerItems.Count > 0 ? OrderStatus.Pending : OrderStatus.Confirmed;

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = request.UserId,
            OrderStatus = orderStatus,
            ItemsTotal = summary.Subtotal,
            Discount = summary.DiscountTotal,
            GstAmount = summary.TaxTotal,
            DeliveryCharge = summary.DeliveryCharge,
            HandlingFee = summary.HandlingFee,
            GrandTotal = summary.Total,
            AppliedCouponId = summary.AppliedCouponCode is null ? null : cart.AppliedCouponId,
            FulfilledByWarehouseId = primaryWarehouseId,
            DealerId = primaryDealerId,
            TrackingNumber = trackingNumber,
            CourierName = courier,
            EstimatedDeliveryDate = estimatedDelivery,
            CurrentLocation = currentLocation ?? address.City,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var item in cart.CartItems)
        {
            Guid? whId = null;
            Guid? dId = item.Product.DealerId;

            if (!dId.HasValue)
            {
                var nearest = ECommercePlatform.Application.Services.WarehouseLocator.FindNearest(
                    allWarehouses,
                    inventories.ToList(),
                    item.ProductId,
                    item.Quantity,
                    address.City ?? string.Empty,
                    address.State ?? string.Empty,
                    address.Latitude == 0 ? null : (double?)address.Latitude,
                    address.Longitude == 0 ? null : (double?)address.Longitude);
                whId = nearest?.WarehouseId;
            }

            order.OrderItems.Add(new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price,
                AllocatedWarehouseId = whId,
                DealerId = dId
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
            PaymentGateway = method == PaymentMethod.COD ? null : "Manual",
            Amount = order.GrandTotal,
            Currency = "INR",
            Status = method == PaymentMethod.COD ? PaymentStatus.Pending : PaymentStatus.Initiated,
            CreatedAt = now,
            UpdatedAt = now
        };
        orders.AddPayment(payment);

        var initialRemarks = dealerItems.Count > 0
            ? $"Order placed for dealer shop. Awaiting confirmation from shop. Tracking: {trackingNumber} via {courier}. Nearest dispatch: {currentLocation}."
            : $"Order confirmed. Tracking: {trackingNumber} via {courier}. Dispatched from {currentLocation}. ETA: {estimatedDelivery:dd MMM yyyy}.";

        orders.AddStatusHistory(new OrderStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Status = order.OrderStatus,
            ChangedByUserId = request.UserId,
            Remarks = initialRemarks,
            ChangedAt = now.UtcDateTime
        });

        foreach (var item in cart.CartItems)
        {
            carts.RemoveItem(item);
        }

        var orderNo = OrderMappings.OrderNoFor(order);
        var notifyMsg = dealerItems.Count > 0
            ? $"{user.Name} placed a shop order {orderNo} for dealer shop. Please confirm."
            : $"{user.Name} placed an order {orderNo} of ₹{order.GrandTotal} ({cart.CartItems.Count} items) from nearest warehouse {currentLocation}.";

        NotificationEmitter.Emit(
            notifications,
            "order",
            $"New order {orderNo}",
            notifyMsg,
            dealerItems.Count > 0 ? $"/admin/agents" : "/admin/orders");

        // Dealer-specific notification
        if (primaryDealerId.HasValue)
        {
            try
            {
                var dealer = await dealers.GetByIdAsync(primaryDealerId.Value, cancellationToken);
                if (dealer is not null)
                {
                    NotificationEmitter.Emit(
                        notifications,
                        "dealer_order",
                        $"New shop order {orderNo}",
                        $"{user.Name} ordered from your shop {dealer.ShopName}. Please confirm and ship.",
                        $"/admin/agents/{dealer.AgentId}/dealers/{dealer.Id}");
                }
            }
            catch { }
        }

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var trackingHtml = $"<p><strong>Tracking:</strong> {trackingNumber} via {courier}<br/><strong>Dispatch:</strong> {currentLocation}<br/><strong>ETA:</strong> {estimatedDelivery:dd MMM yyyy}</p>";
        await OrderEmailSender.TrySendAsync(
            emailService,
            logger,
            user.Email,
            $"Order {orderNo} confirmed - {trackingNumber}",
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
                addressSummary) + trackingHtml,
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

        // Dealer products are stocked at the shop, not in warehouse inventory — skip warehouse stock check
        var warehouseProductIds = cart.CartItems.Where(i => !i.Product.DealerId.HasValue).Select(i => i.ProductId).Distinct().ToArray();
        if (warehouseProductIds.Length == 0) return Array.Empty<string>();

        var availableByProduct = (await inventory.GetByProductIdsAsync(warehouseProductIds, cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.StockQuantity - i.ReservedQuantity));

        return cart.CartItems
            .Where(i => !i.Product.DealerId.HasValue && availableByProduct.TryGetValue(i.ProductId, out var available) && i.Quantity > available)
            .Select(i => i.Product.ProductName)
            .ToArray();
    }
}
