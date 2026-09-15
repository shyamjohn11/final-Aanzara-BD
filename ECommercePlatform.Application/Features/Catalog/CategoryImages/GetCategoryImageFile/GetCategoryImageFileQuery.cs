using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImageFile;

/// <summary>
/// Resolves a category's primary image file by category id alone — the caller
/// never needs to know a specific image id for the common "show me the
/// picture for this category" case.
/// </summary>
public sealed record GetCategoryImageFileQuery(Guid CategoryId)
    : IQuery<Result<CategoryImageFileResponse>>;