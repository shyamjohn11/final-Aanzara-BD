using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.SubCategoryImages.GetSubCategoryImages;

public sealed class GetSubCategoryImagesQueryHandler
    : IQueryHandler<GetSubCategoryImagesQuery, Result<IReadOnlyCollection<SubCategoryImageResponse>>>
{
    private readonly ISubCategoryImageRepository _images;
    private readonly ISubCategoryRepository _subCategories;
    private readonly ILogger<GetSubCategoryImagesQueryHandler> _logger;

    public GetSubCategoryImagesQueryHandler(
        ISubCategoryImageRepository images,
        ISubCategoryRepository subCategories,
        ILogger<GetSubCategoryImagesQueryHandler> logger)
    {
        _images = images;
        _subCategories = subCategories;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyCollection<SubCategoryImageResponse>>> Handle(
        GetSubCategoryImagesQuery request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategories.GetByIdAsync(request.SubCategoryId, cancellationToken);

        if (subCategory is null)
        {
            return Result.Failure<IReadOnlyCollection<SubCategoryImageResponse>>(
                CatalogErrors.SubCategoryNotFound);
        }

        var images = await _images.GetForSubCategoryAsync(request.SubCategoryId, cancellationToken);

        return Result.Success<IReadOnlyCollection<SubCategoryImageResponse>>(
            images.OrderBy(i => i.DisplayOrder).Select(i => i.ToResponse()).ToArray());
    }
}