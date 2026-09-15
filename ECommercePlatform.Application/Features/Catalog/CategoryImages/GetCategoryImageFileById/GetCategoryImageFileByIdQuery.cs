using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImageFileById;

public sealed record GetCategoryImageFileByIdQuery(Guid CategoryImageId)
    : IQuery<Result<CategoryImageFileResponse>>;
