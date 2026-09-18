namespace ECommercePlatform.Application.Features.Wishlist.Dtos;

public sealed record WishlistItemResponse(
    Guid WishlistItemId,
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal Price,
    decimal Mrp,
    string Status,
    string ImageUrl);