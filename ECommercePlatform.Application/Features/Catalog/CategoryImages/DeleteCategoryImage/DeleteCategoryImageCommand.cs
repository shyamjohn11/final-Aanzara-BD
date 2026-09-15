using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.DeleteCategoryImage;

public sealed record DeleteCategoryImageCommand(Guid CategoryImageId) : ICommand<Result>;
