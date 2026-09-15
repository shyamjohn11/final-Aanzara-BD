using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategoryImages.GetSubCategoryImages;

public sealed record GetSubCategoryImagesQuery(Guid SubCategoryId)
    : IQuery<Result<IReadOnlyCollection<SubCategoryImageResponse>>>;