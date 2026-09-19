using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetProducts;

public sealed class GetProductsQueryHandler
    : IQueryHandler<GetProductsQuery, Result<PagedResult<ProductSummaryResponse>>>
{
    private readonly IProductRepository _products;
    private readonly IBrandRepository _brands;
    private readonly IProductImageRepository _productImages;
    private readonly IInventoryRepository _inventory;

    public GetProductsQueryHandler(
        IProductRepository products,
        IBrandRepository brands,
        IProductImageRepository productImages,
        IInventoryRepository inventory)
    {
        _products = products;
        _brands = brands;
        _productImages = productImages;
        _inventory = inventory;
    }

    public async Task<Result<PagedResult<ProductSummaryResponse>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.Status is not null && !ProductStatus.IsValid(request.Status))
        {
            return Result.Failure<PagedResult<ProductSummaryResponse>>(
                CatalogErrors.InvalidStatus(request.Status));
        }

        var filter = new ProductFilter(
            CategoryId: request.CategoryId,
            SubCategoryId: request.SubCategoryId,
            BrandId: request.BrandId,
            Search: request.Search?.Trim(),
            Status: request.Status,
            MinPrice: request.MinPrice,
            MaxPrice: request.MaxPrice,
            SortBy: request.SortBy,
            SortDescending: request.SortDescending);

        var page = await _products.SearchAsync(
            filter, request.Page, request.PageSize, cancellationToken);

        var enriched = await page.Items.ToEnrichedSummariesAsync(
            _brands, _productImages, _inventory, cancellationToken);

        return Result.Success(new PagedResult<ProductSummaryResponse>(
            enriched.ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}