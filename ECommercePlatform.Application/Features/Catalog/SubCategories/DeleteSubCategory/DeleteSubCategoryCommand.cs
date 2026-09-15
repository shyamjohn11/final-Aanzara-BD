using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategories.DeleteSubCategory;

public sealed record DeleteSubCategoryCommand(Guid SubCategoryId) : ICommand<Result>;
