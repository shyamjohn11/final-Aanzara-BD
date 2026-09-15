using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.AddCategoryImage;

public sealed class AddCategoryImageCommandHandler
    : ICommandHandler<AddCategoryImageCommand, Result<CategoryImageResponse>>
{
    private readonly ICategoryImageRepository _images;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddCategoryImageCommandHandler> _logger;

    public AddCategoryImageCommandHandler(
        ICategoryImageRepository images,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        ILogger<AddCategoryImageCommandHandler> logger)
    {
        _images = images;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryImageResponse>> Handle(
        AddCategoryImageCommand request, CancellationToken cancellationToken)
    {
        if (!await _categories.ExistsAsync(request.CategoryId, cancellationToken))
        {
            return Result.Failure<CategoryImageResponse>(CatalogErrors.CategoryNotFound);
        }

        var existing = await _images.GetForCategoryAsync(request.CategoryId, cancellationToken);

        // The first image becomes primary automatically — a category with images
        // but no primary would leave listings with nothing to show.
        var isPrimary = request.IsPrimary || existing.Count == 0;

        if (isPrimary)
        {
            await _images.ClearPrimaryAsync(request.CategoryId, cancellationToken);
        }

        var image = new CategoryImage
        {
            CategoryImageId = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl.Trim(),
            FileName = request.FileName.Trim(),
            FilePath = request.FilePath.Trim(),
            ContentType = request.ContentType.Trim(),
            FileSize = request.FileSize,
            IsPrimary = isPrimary,
            DisplayOrder = await _images.NextDisplayOrderAsync(request.CategoryId, cancellationToken)
        };

        _images.Add(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Image {ImageId} added to category {CategoryId}.", image.CategoryImageId, request.CategoryId);

        return Result.Success(image.ToResponse());
    }
}
