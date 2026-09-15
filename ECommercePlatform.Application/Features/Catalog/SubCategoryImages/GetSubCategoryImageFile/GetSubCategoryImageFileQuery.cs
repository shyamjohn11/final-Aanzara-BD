using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategoryImages.GetSubCategoryImageFile;

public sealed record GetSubCategoryImageFileQuery(Guid SubCategoryId)
    : IQuery<Result<CategoryImageFileResponse>>;
