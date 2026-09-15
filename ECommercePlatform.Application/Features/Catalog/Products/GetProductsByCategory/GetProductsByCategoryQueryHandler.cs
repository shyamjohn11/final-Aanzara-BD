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

    public GetProductsByCategoryQueryHandler(IProductRepository products) => _products = products;

    public async Task<Result<PagedResult<ProductSummaryResponse>>> Handle(
        GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var filter = new ProductFilter(
            CategoryId: request.CategoryId,
            Status: ProductStatus.Active);

        var page = await _products.SearchAsync(
            filter, request.Page, request.PageSize, cancellationToken);

        return Result.Success(new PagedResult<ProductSummaryResponse>(
            page.Items.Select(p => p.ToSummary()).ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}