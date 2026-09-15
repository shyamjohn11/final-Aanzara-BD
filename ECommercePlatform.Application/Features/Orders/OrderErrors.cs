using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Orders;

public static class OrderErrors
{
    public static readonly Error NotAuthenticated =
        Error.Unauthorized("orders.not_authenticated", "You must be signed in to view your orders.");

    public static readonly Error NotFound =
        Error.NotFound("orders.not_found", "Order not found.");

    public static readonly Error EmptyCart =
        Error.Validation("orders.empty_cart", "Your cart is empty.");

    public static readonly Error InvalidPaymentMethod =
        Error.Validation("orders.invalid_payment_method", "Unsupported payment method.");

    public static readonly Error PaymentNotFound =
        Error.NotFound("orders.payment_not_found", "No payment record exists for this order.");

    public static Error InsufficientStock(string productNames) =>
        Error.Validation(
            "orders.insufficient_stock",
            $"Not enough stock left for: {productNames}.");

    public static Error NotCancellable(string status) =>
        Error.Validation(
            "orders.not_cancellable",
            $"An order in status '{status}' can no longer be cancelled.");
}
