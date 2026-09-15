using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Products.CreateProduct;

public sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly ISubCategoryRepository _subCategories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository products,
        ICategoryRepository categories,
        ISubCategoryRepository subCategories,
        IUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _products = products;
        _categories = categories;
        _subCategories = subCategories;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductResponse>> Handle(
        CreateProductCommand request, CancellationToken cancellationToken)
    {
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

        if (await _products.SkuExistsAsync(sku, excludingId: null, cancellationToken))
        {
            return Result.Failure<ProductResponse>(CatalogErrors.SkuTaken(sku));
        }

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,
            BrandId = request.BrandId,
            ProductName = request.ProductName.Trim(),
            Sku = sku,
            Description = request.Description?.Trim(),
            Price = request.Price,
            Mrp = request.Mrp,
            Discount = request.Discount,
            Moq = request.Moq,
            IsOrganic = request.IsOrganic,
            IsGstFree = request.IsGstFree,
            Status = request.Status
        };

        _products.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} ({Sku}) created.", product.ProductId, sku);

        return Result.Success(product.ToResponse());
    }
}