using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist;

public static class WishlistErrors
{
    public static readonly Error NotAuthenticated =
        Error.Unauthorized("wishlist.not_authenticated", "You must be signed in to use your wishlist.");

    public static readonly Error ProductUnavailable =
        Error.Validation("wishlist.product_unavailable", "The selected product is not available.");

    public static readonly Error AlreadyExists =
        Error.Conflict("wishlist.already_exists", "This product is already in your wishlist.");

    public static readonly Error ItemNotFound =
        Error.NotFound("wishlist.item_not_found", "This product is not in your wishlist.");
}