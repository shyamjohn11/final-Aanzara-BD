using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.GetBrands;

public sealed class GetBrandsQueryHandler
    : IQueryHandler<GetBrandsQuery, Result<PagedResult<BrandResponse>>>
{
    private readonly IBrandRepository _brands;

    public GetBrandsQueryHandler(IBrandRepository brands) => _brands = brands;

    public async Task<Result<PagedResult<BrandResponse>>> Handle(
        GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var page = await _brands.SearchAsync(
            request.Search?.Trim(), request.Status, request.Page, request.PageSize, cancellationToken);

        return Result.Success(new PagedResult<BrandResponse>(
            page.Items.Select(b => b.ToResponse()).ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}