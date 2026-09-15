using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId) : ICommand<Result>;
