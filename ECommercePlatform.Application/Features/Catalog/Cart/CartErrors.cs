using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart;

public static class CartErrors
{
    public static readonly Error NotAuthenticated =
        Error.Unauthorized("cart.not_authenticated", "You must be signed in to view your cart.");

    public static readonly Error InvalidQuantity =
        Error.Validation("cart.invalid_quantity", "Quantity must be greater than zero.");

    public static readonly Error ProductUnavailable =
        Error.Validation("cart.product_unavailable", "This product is currently unavailable.");

    public static readonly Error InsufficientStock =
        Error.Validation("cart.insufficient_stock", "The requested quantity is not available in stock.");

    public static readonly Error ItemNotFound =
        Error.NotFound("cart.item_not_found", "Cart item not found.");
}