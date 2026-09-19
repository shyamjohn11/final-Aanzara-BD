using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Shop.Brands;

/// <summary>
/// Public storefront brand shelf. Active brands only, newest first — no
/// authentication required, mirroring the product shelf endpoints. The
/// admin brand console keeps its own permission-gated endpoints.
/// </summary>
public sealed record GetActiveBrandsQuery(string? Search = null, int Count = 50)
    : IQuery<Result<IReadOnlyList<BrandResponse>>>;

public sealed class GetActiveBrandsQueryHandler(
    IBrandRepository brands)
    : IQueryHandler<GetActiveBrandsQuery, Result<IReadOnlyList<BrandResponse>>>
{
    public async Task<Result<IReadOnlyList<BrandResponse>>> Handle(
        GetActiveBrandsQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 50 : request.Count, 1, 100);

        var page = await brands.SearchAsync(
            request.Search?.Trim(),
            BrandStatus.Active,
            page: 1,
            pageSize: count,
            cancellationToken);

        return Result.Success<IReadOnlyList<BrandResponse>>(
            page.Items.Select(b => b.ToResponse()).ToArray());
    }
}
