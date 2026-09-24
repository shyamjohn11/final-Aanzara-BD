using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Application.Features.Admin.Dealers;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Application.Features.Catalog.Products;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.DealerProducts;

// Products owned by one dealer (Product.DealerId). Reuses the catalog's own
// validation (ProductWriteModel shape + category/sub-category placement +
// SKU uniqueness) plus dealer scoping: every write proves the dealer exists
// and the product belongs to it, so products can never leak between dealers.
// Legacy/global products keep DealerId null and are untouched by this module.

// IDs #158-161 — GET list / POST / PUT / DELETE under a dealer, Admin-role
// (plus GET by id for the 201 CreatedAtRoute target).

public sealed record GetDealerProductsQuery : IQuery<Result<PagedResult<ProductSummaryResponse>>>
{
    public Guid DealerId { get; init; }
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record CreateDealerProductCommand : ProductWriteModel, ICommand<Result<ProductResponse>>
{
    /// <summary>Comes from the route, not the body — never trusted from the client.</summary>
    [JsonIgnore]
    public Guid DealerId { get; init; }
}

public sealed record UpdateDealerProductCommand : ProductWriteModel, ICommand<Result<ProductResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid DealerId { get; init; }

    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid ProductId { get; init; }
}

public sealed record DeleteDealerProductCommand(Guid DealerId, Guid ProductId) : ICommand<Result>;

public sealed record GetDealerProductByIdQuery(Guid DealerId, Guid ProductId)
    : IQuery<Result<ProductResponse>>;

internal static class DealerProductErrors
{
    public static Error DealerProductNotFound(Guid productId) => Error.NotFound(
        "admin.dealer_product_not_found", $"Product '{productId}' was not found for this dealer.");
}

public sealed class GetDealerProductsQueryHandler(
    IAdminRepository<Dealer> dealers,
    IProductRepository products)
    : IQueryHandler<GetDealerProductsQuery, Result<PagedResult<ProductSummaryResponse>>>
{
    public async Task<Result<PagedResult<ProductSummaryResponse>>> Handle(
        GetDealerProductsQuery request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.DealerId, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure<PagedResult<ProductSummaryResponse>>(
                AdminErrors.NotFound("Dealer", request.DealerId));
        }

        var filter = new ProductFilter(
            DealerId: dealer.Id,
            Search: request.Search?.Trim(),
            Status: request.Status);

        var page = await products.SearchAsync(
            filter,
            Math.Max(1, request.Page),
            Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200),
            cancellationToken);

        return Result.Success(new PagedResult<ProductSummaryResponse>(
            page.Items.Select(p => p.ToSummary()).ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}

public sealed class CreateDealerProductCommandHandler(
    IAdminRepository<Dealer> dealers,
    IProductRepository products,
    ICategoryRepository categories,
    ISubCategoryRepository subCategories,
    IBrandRepository brands,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDealerProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(
        CreateDealerProductCommand request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.DealerId, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure<ProductResponse>(AdminErrors.NotFound("Dealer", request.DealerId));
        }

        if (dealer.Status != DealerStatus.Active)
        {
            return Result.Failure<ProductResponse>(Error.Validation(
                "admin.dealer_not_active", $"Dealer '{dealer.ShopName}' is not active."));
        }

        if (ProductValidation.CheckShape(request) is { } shapeError)
        {
            return Result.Failure<ProductResponse>(shapeError);
        }

        if (await ProductValidation.CheckPlacementAsync(
                request, categories, subCategories, cancellationToken) is { } placementError)
        {
            return Result.Failure<ProductResponse>(placementError);
        }

        if (request.BrandId.HasValue
            && !await brands.ExistsAsync(request.BrandId.Value, cancellationToken))
        {
            return Result.Failure<ProductResponse>(CatalogErrors.BrandNotFound);
        }

        var sku = request.Sku.Trim();

        if (await products.SkuExistsAsync(sku, excludingId: null, cancellationToken))
        {
            return Result.Failure<ProductResponse>(CatalogErrors.SkuTaken(sku));
        }

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,
            BrandId = request.BrandId,
            DealerId = dealer.Id,
            ProductName = request.ProductName.Trim(),
            Sku = sku,
            Description = request.Description?.Trim(),
            Specification = request.Specification?.Trim(),
            Price = request.Price,
            Mrp = request.Mrp,
            Discount = request.Discount,
            Moq = request.Moq,
            IsOrganic = request.IsOrganic,
            IsGstFree = request.IsGstFree,
            Status = request.Status
        };

        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.ToResponse());
    }
}

public sealed class UpdateDealerProductCommandHandler(
    IAdminRepository<Dealer> dealers,
    IProductRepository products,
    ICategoryRepository categories,
    ISubCategoryRepository subCategories,
    IBrandRepository brands,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDealerProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(
        UpdateDealerProductCommand request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.DealerId, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure<ProductResponse>(AdminErrors.NotFound("Dealer", request.DealerId));
        }

        var product = await products.GetByIdAsync(request.ProductId, cancellationToken);

        // Belonging is proved against the database row, not the request:
        // a product of another dealer (or a global product) is a 404 here.
        if (product is null || product.DealerId != dealer.Id)
        {
            return Result.Failure<ProductResponse>(
                DealerProductErrors.DealerProductNotFound(request.ProductId));
        }

        if (ProductValidation.CheckShape(request) is { } shapeError)
        {
            return Result.Failure<ProductResponse>(shapeError);
        }

        if (await ProductValidation.CheckPlacementAsync(
                request, categories, subCategories, cancellationToken) is { } placementError)
        {
            return Result.Failure<ProductResponse>(placementError);
        }

        if (request.BrandId.HasValue
            && !await brands.ExistsAsync(request.BrandId.Value, cancellationToken))
        {
            return Result.Failure<ProductResponse>(CatalogErrors.BrandNotFound);
        }

        var sku = request.Sku.Trim();

        if (await products.SkuExistsAsync(sku, request.ProductId, cancellationToken))
        {
            return Result.Failure<ProductResponse>(CatalogErrors.SkuTaken(sku));
        }

        product.CategoryId = request.CategoryId;
        product.SubCategoryId = request.SubCategoryId;
        product.BrandId = request.BrandId;
        product.ProductName = request.ProductName.Trim();
        product.Sku = sku;
        product.Description = request.Description?.Trim();
        product.Specification = request.Specification?.Trim();
        product.Price = request.Price;
        product.Mrp = request.Mrp;
        product.Discount = request.Discount;
        product.Moq = request.Moq;
        product.IsOrganic = request.IsOrganic;
        product.IsGstFree = request.IsGstFree;
        product.Status = request.Status;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.ToResponse());
    }
}

public sealed class DeleteDealerProductCommandHandler(
    IAdminRepository<Dealer> dealers,
    IProductRepository products,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDealerProductCommand, Result>
{
    public async Task<Result> Handle(DeleteDealerProductCommand request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.DealerId, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure(AdminErrors.NotFound("Dealer", request.DealerId));
        }

        var product = await products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.DealerId != dealer.Id)
        {
            return Result.Failure(DealerProductErrors.DealerProductNotFound(request.ProductId));
        }

        products.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class GetDealerProductByIdQueryHandler(
    IAdminRepository<Dealer> dealers,
    IProductRepository products)
    : IQueryHandler<GetDealerProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(
        GetDealerProductByIdQuery request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.DealerId, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure<ProductResponse>(AdminErrors.NotFound("Dealer", request.DealerId));
        }

        var product = await products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.DealerId != dealer.Id)
        {
            return Result.Failure<ProductResponse>(
                DealerProductErrors.DealerProductNotFound(request.ProductId));
        }

        return Result.Success(product.ToResponse());
    }
}
