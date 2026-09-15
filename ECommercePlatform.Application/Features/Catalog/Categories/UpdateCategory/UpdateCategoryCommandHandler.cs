using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : ICommandHandler<UpdateCategoryCommand, Result<CategoryResponse>>
{
    private const string ImageSubFolder = "Categories";

    private readonly ICategoryRepository _categories;
    private readonly ICategoryImageRepository _images;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categories,
        ICategoryImageRepository images,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categories = categories;
        _images = images;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryResponse>> Handle(
        UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure<CategoryResponse>(CatalogErrors.CategoryNotFound);
        }

        var code = request.CategoryCode.Trim();

        // Excluding this row's own id lets a category keep its existing code.
        if (await _categories.CodeExistsAsync(code, request.CategoryId, cancellationToken))
        {
            return Result.Failure<CategoryResponse>(CatalogErrors.CategoryCodeTaken(code));
        }

        category.CategoryCode = code;
        category.CategoryName = request.CategoryName.Trim();
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;

        // Saved so field changes commit even if the image upload below fails.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Image is not null)
        {
            var stored = await _fileStorage.SaveAsync(request.Image, ImageSubFolder, cancellationToken);

            // The freshly uploaded image becomes the primary; the previous
            // primary (if any) stays in the gallery demoted to non-primary.
            await _images.ClearPrimaryAsync(category.CategoryId, cancellationToken);

            var image = new CategoryImage
            {
                CategoryImageId = Guid.NewGuid(),
                CategoryId = category.CategoryId,
                ImageUrl = stored.Url,
                FileName = stored.FileName,
                FilePath = stored.FilePath,
                ContentType = stored.ContentType,
                FileSize = stored.FileSize,
                IsPrimary = true,
                DisplayOrder = await _images.NextDisplayOrderAsync(category.CategoryId, cancellationToken)
            };

            _images.Add(image);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Delete the newly uploaded file if the DB save fails, so a
                // failed update never leaves an orphan file behind.
                await _fileStorage.DeleteAsync(stored.FilePath, cancellationToken);
                throw;
            }
        }

        _logger.LogInformation("Category {CategoryId} updated.", category.CategoryId);

        return Result.Success(category.ToResponse());
    }
}