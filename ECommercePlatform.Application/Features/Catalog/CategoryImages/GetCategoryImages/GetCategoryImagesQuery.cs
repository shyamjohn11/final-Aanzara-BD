using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImages;

public sealed record GetCategoryImagesQuery(Guid CategoryId)
    : IQuery<Result<IReadOnlyCollection<CategoryImageResponse>>>;
