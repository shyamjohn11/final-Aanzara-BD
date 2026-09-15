using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.GetBrandById;

public sealed record GetBrandByIdQuery(Guid BrandId) : IQuery<Result<BrandResponse>>;