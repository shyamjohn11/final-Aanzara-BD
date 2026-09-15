using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.GetProductImageFile;

public sealed record GetProductImageFileQuery(Guid ProductId, Guid ImageId)
    : IQuery<Result<CategoryImageFileResponse>>;
