using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.GetBrandById;

public sealed class GetBrandByIdQueryHandler
    : IQueryHandler<GetBrandByIdQuery, Result<BrandResponse>>
{
    private readonly IBrandRepository _brands;

    public GetBrandByIdQueryHandler(IBrandRepository brands) => _brands = brands;

    public async Task<Result<BrandResponse>> Handle(
        GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await _brands.GetWithImagesAsync(request.BrandId, cancellationToken);

        return brand is null
            ? Result.Failure<BrandResponse>(CatalogErrors.BrandNotFound)
            : Result.Success(brand.ToResponse());
    }
}