using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.SubCategories.CreateSubCategory;

public sealed class CreateSubCategoryCommandHandler
    : ICommandHandler<CreateSubCategoryCommand, Result<SubCategoryResponse>>
{
    private readonly ISubCategoryRepository _subCategories;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSubCategoryCommandHandler> _logger;

    public CreateSubCategoryCommandHandler(
        ISubCategoryRepository subCategories,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        ILogger<CreateSubCategoryCommandHandler> logger)
    {
        _subCategories = subCategories;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubCategoryResponse>> Handle(
        CreateSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var parent = await _categories.GetByIdAsync(request.CategoryId, cancellationToken);

        if (parent is null)
        {
            return Result.Failure<SubCategoryResponse>(CatalogErrors.CategoryNotFound);
        }

        var code = request.SubCategoryCode.Trim();

        if (await _subCategories.CodeExistsAsync(code, excludingId: null, cancellationToken))
        {
            return Result.Failure<SubCategoryResponse>(CatalogErrors.SubCategoryCodeTaken(code));
        }

        var subCategory = new SubCategory
        {
            SubCategoryId = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            SubCategoryCode = code,
            SubCategoryName = request.SubCategoryName.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        _subCategories.Add(subCategory);

        // Keep the denormalized flag true to the data now that a child exists.
        parent.HasSubCategory = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sub-category {SubCategoryId} created under category {CategoryId}.",
            subCategory.SubCategoryId, request.CategoryId);

        return Result.Success(subCategory.ToResponse());
    }
}
