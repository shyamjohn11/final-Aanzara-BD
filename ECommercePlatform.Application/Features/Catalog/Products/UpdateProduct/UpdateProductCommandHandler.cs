using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : ICommandHandler<UpdateProductCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly ISubCategoryRepository _subCategories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository products,
        ICategoryRepository categories,
        ISubCategoryRepository subCategories,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _products = products;
        _categories = categories;
        _subCategories = subCategories;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductResponse>> Handle(
        UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(CatalogErrors.ProductNotFound);
        }

        if (ProductValidation.CheckShape(request) is { } shapeError)
        {
            return Result.Failure<ProductResponse>(shapeError);
        }

        if (await ProductValidation.CheckPlacementAsync(
                request, _categories, _subCategories, cancellationToken) is { } placementError)
        {
            return Result.Failure<ProductResponse>(placementError);
        }

        var sku = request.Sku.Trim();

        if (await _products.SkuExistsAsync(sku, request.ProductId, cancellationToken))
        {
            return Result.Failure<ProductResponse>(CatalogErrors.SkuTaken(sku));
        }

        product.CategoryId = request.CategoryId;
        product.SubCategoryId = request.SubCategoryId;
        product.BrandId = request.BrandId;
        product.ProductName = request.ProductName.Trim();
        product.Sku = sku;
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Mrp = request.Mrp;
        product.Discount = request.Discount;
        product.Moq = request.Moq;
        product.IsOrganic = request.IsOrganic;
        product.IsGstFree = request.IsGstFree;
        product.Status = request.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} updated.", product.ProductId);

        return Result.Success(product.ToResponse());
    }
}