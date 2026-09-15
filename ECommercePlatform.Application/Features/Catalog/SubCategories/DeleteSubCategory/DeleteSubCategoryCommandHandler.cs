using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.SubCategories.DeleteSubCategory;

public sealed class DeleteSubCategoryCommandHandler : ICommandHandler<DeleteSubCategoryCommand, Result>
{
    private readonly ISubCategoryRepository _subCategories;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSubCategoryCommandHandler> _logger;

    public DeleteSubCategoryCommandHandler(
        ISubCategoryRepository subCategories,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        ILogger<DeleteSubCategoryCommandHandler> logger)
    {
        _subCategories = subCategories;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategories.GetByIdAsync(request.SubCategoryId, cancellationToken);

        if (subCategory is null)
        {
            return Result.Failure(CatalogErrors.SubCategoryNotFound);
        }

        if (await _subCategories.HasProductsAsync(request.SubCategoryId, cancellationToken))
        {
            return Result.Failure(CatalogErrors.SubCategoryHasProducts);
        }

        var parentId = subCategory.CategoryId;

        _subCategories.Remove(subCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Removing the last child flips HasSubCategory back off. Recomputed from
        // the rows rather than decremented, so it cannot drift.
        await _categories.RefreshHasSubCategoryAsync(parentId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sub-category {SubCategoryId} deleted.", request.SubCategoryId);

        return Result.Success();
    }
}
