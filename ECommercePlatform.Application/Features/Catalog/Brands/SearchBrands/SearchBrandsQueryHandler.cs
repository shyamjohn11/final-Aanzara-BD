using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.SearchBrands;

public sealed class SearchBrandsQueryHandler
    : IQueryHandler<SearchBrandsQuery, Result<IReadOnlyList<BrandResponse>>>
{
    private readonly IBrandRepository _brands;

    public SearchBrandsQueryHandler(IBrandRepository brands) => _brands = brands;

    public async Task<Result<IReadOnlyList<BrandResponse>>> Handle(
        SearchBrandsQuery request, CancellationToken cancellationToken)
    {
        var page = await _brands.SearchAsync(
            request.Keyword?.Trim(), BrandStatus.Active, page: 1, request.Limit, cancellationToken);

        return Result.Success<IReadOnlyList<BrandResponse>>(
            page.Items.Select(b => b.ToResponse()).ToArray());
    }
}