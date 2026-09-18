using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist.AddToWishlist;

public sealed class AddToWishlistCommandHandler : ICommandHandler<AddToWishlistCommand, Result>
{
    private readonly IWishlistRepository _wishlists;
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;

    public AddToWishlistCommandHandler(
        IWishlistRepository wishlists, IProductRepository products, IUnitOfWork unitOfWork)
    {
        _wishlists = wishlists;
        _products = products;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(CatalogErrors.ProductNotFound);
        }

        // Status is free text in the schema, so rows can differ in casing (older
        // imports wrote "active") — compare case-insensitively like ProductStatus.IsValid.
        if (!string.Equals(product.Status, ProductStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(WishlistErrors.ProductUnavailable);
        }

        var existing = await _wishlists.GetByUserAndProductAsync(
            request.UserId, request.ProductId, cancellationToken);

        if (existing is not null)
        {
            return Result.Failure(WishlistErrors.AlreadyExists);
        }

        _wishlists.Add(new WishlistItem
        {
            WishlistItemId = Guid.NewGuid(),
            UserId = request.UserId,
            ProductId = request.ProductId
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}