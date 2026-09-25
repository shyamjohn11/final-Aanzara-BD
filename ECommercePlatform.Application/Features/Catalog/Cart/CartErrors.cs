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

    public static readonly Error EmptyCart =
        Error.Validation("cart.empty", "Your cart is empty.");

    public static readonly Error CouponNotFound =
        Error.NotFound("cart.coupon_not_found", "Invalid coupon code.");

    public static readonly Error CouponInactive =
        Error.Validation("cart.coupon_inactive", "This coupon is no longer active.");

    public static readonly Error CouponExpired =
        Error.Validation("cart.coupon_expired", "This coupon has expired or is not active yet.");

    public static readonly Error CouponNotApplicable =
        Error.Validation("cart.coupon_not_applicable", "This coupon cannot be applied to your cart.");
}