using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.BrandImages.GetBrandImageFile;

public sealed record GetBrandImageFileQuery(Guid BrandId)
    : IQuery<Result<CategoryImageFileResponse>>;
