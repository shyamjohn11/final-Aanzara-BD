using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Categories.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : ICommandHandler<CreateCategoryCommand, Result<CategoryResponse>>
{
    private const string ImageSubFolder = "Categories";

    private readonly ICategoryRepository _categories;
    private readonly ICategoryImageRepository _images;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        ICategoryRepository categories,
        ICategoryImageRepository images,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _categories = categories;
        _images = images;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryResponse>> Handle(
        CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.Image is null)
            {
                return Result.Failure<CategoryResponse>(CatalogErrors.RequiredImage);
            }

            var code = request.CategoryCode.Trim();

        if (await _categories.CodeExistsAsync(code, excludingId: null, cancellationToken))
        {
            return Result.Failure<CategoryResponse>(CatalogErrors.CategoryCodeTaken(code));
        }

        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryCode = code,
            CategoryName = request.CategoryName.Trim(),
            Description = request.Description?.Trim(),
            // No children exist yet; the flag is maintained as sub-categories come and go.
            HasSubCategory = request.HasSubCategory,
            IsActive = request.IsActive
        };

        _categories.Add(category);

        // Saved now so the category has a row for the image to attach to below.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Image is not null)
        {
            var stored = await _fileStorage.SaveAsync(request.Image, ImageSubFolder, cancellationToken);

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
                DisplayOrder = 1
            };

            _images.Add(image);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Delete the newly uploaded file if the DB save fails, so a
                // failed create never leaves an orphan file behind.
                await _fileStorage.DeleteAsync(stored.FilePath, cancellationToken);
                throw;
            }
        }

        _logger.LogInformation(
            "Category {CategoryId} ({CategoryCode}) created.", category.CategoryId, code);

        return Result.Success(category.ToResponse());
    }
}