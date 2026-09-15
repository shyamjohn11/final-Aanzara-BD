using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.SubCategoryImages.SetSubCategoryImage;

public sealed class SetSubCategoryImageCommandHandler
    : ICommandHandler<SetSubCategoryImageCommand, Result<SubCategoryResponse>>
{
    private const string ImageSubFolder = "SubCategories";

    private readonly ISubCategoryRepository _subCategories;
    private readonly ISubCategoryImageRepository _images;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetSubCategoryImageCommandHandler> _logger;

    public SetSubCategoryImageCommandHandler(
        ISubCategoryRepository subCategories,
        ISubCategoryImageRepository images,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<SetSubCategoryImageCommandHandler> logger)
    {
        _subCategories = subCategories;
        _images = images;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubCategoryResponse>> Handle(
        SetSubCategoryImageCommand request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategories.GetByIdAsync(request.SubCategoryId, cancellationToken);

        if (subCategory is null)
        {
            return Result.Failure<SubCategoryResponse>(CatalogErrors.SubCategoryNotFound);
        }

        if (request.File is null || request.File.Length <= 0)
        {
            return Result.Failure<SubCategoryResponse>(Error.Validation(
                "catalog.subcategory_image_required", "An image file is required."));
        }

        if (!request.File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<SubCategoryResponse>(Error.Validation(
                "catalog.subcategory_image_invalid_type", "Only image files are accepted."));
        }

        var stored = await _fileStorage.SaveAsync(request.File, ImageSubFolder, cancellationToken);

        var existing = await _images.GetForSubCategoryAsync(request.SubCategoryId, cancellationToken);
        var primary = existing.FirstOrDefault(i => i.IsPrimary) ?? existing.FirstOrDefault();
        var oldImageUrl = primary?.ImageUrl;

        if (primary is not null)
        {
            // Same row, new file: keeps ImageId stable for anything referencing it.
            primary.ImageUrl = stored.Url;
            primary.IsPrimary = true;
        }
        else
        {
            _images.Add(new SubCategoryImage
            {
                ImageId = Guid.NewGuid(),
                SubCategoryId = request.SubCategoryId,
                ImageUrl = stored.Url,
                IsPrimary = true,
                DisplayOrder = 0
            });
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // A failed save must never leave an orphan file behind.
            await _fileStorage.DeleteByUrlAsync(stored.Url, cancellationToken);
            throw;
        }

        // Only remove the old file once the DB update commits.
        if (!string.IsNullOrWhiteSpace(oldImageUrl) && oldImageUrl != stored.Url)
        {
            await _fileStorage.DeleteByUrlAsync(oldImageUrl, cancellationToken);
        }

        _logger.LogInformation(
            "Image saved for sub-category {SubCategoryId}.", request.SubCategoryId);

        return Result.Success(subCategory.ToResponse());
    }
}
