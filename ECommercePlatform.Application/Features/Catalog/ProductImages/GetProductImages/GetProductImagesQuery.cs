using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.GetProductImages;

public sealed record GetProductImagesQuery(Guid ProductId)
    : IQuery<Result<IReadOnlyList<ProductImageResponse>>>;
