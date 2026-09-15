using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.SetPrimaryImage;

public sealed record SetPrimaryImageCommand(Guid CategoryImageId)
    : ICommand<Result<CategoryImageResponse>>;
