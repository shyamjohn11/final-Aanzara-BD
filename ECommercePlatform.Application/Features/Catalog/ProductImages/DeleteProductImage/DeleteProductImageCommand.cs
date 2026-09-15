using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.DeleteProductImage;

public sealed record DeleteProductImageCommand(Guid ProductId, Guid ImageId)
    : ICommand<Result>;
