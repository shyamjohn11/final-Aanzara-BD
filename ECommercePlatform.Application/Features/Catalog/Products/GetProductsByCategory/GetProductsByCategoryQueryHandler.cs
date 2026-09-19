using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;


namespace ECommercePlatform.Application.Features.Catalog.Products.GetProductsByCategory;

public sealed class GetProductsByCategoryQueryHandler
    : IQueryHandler<GetProductsByCategoryQuery, Result<PagedResult<ProductSummaryResponse>>>
{
    private readonly IProductRepository _products;
    private readonly IBrandRepository _brands;
    private readonly IProductImageRepository _productImages;
    private readonly IInventoryRepository _inventory;

    public GetProductsByCategoryQueryHandler(
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
        GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var filter = new ProductFilter(
            CategoryId: request.CategoryId,
            Status: ProductStatus.Active);

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