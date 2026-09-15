using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.SetPrimaryProductImage;

/// <summary>Promotes an image to primary, demoting the incumbent.</summary>
public sealed record SetPrimaryProductImageCommand(Guid ProductId, Guid ImageId)
    : ICommand<Result<ProductImageResponse>>;
