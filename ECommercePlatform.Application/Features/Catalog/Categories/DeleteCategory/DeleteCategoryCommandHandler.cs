using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Categories.DeleteCategory;

public sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetWithImagesAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CatalogErrors.CategoryNotFound);
        }

        // Refuse rather than cascade: silently deleting a subtree of products is
        // never what the caller meant. Images go with the category, since they
        // cannot outlive it.
        if (category.SubCategories.Count > 0 || category.Products.Count > 0)
        {
            _logger.LogInformation(
                "Refused to delete category {CategoryId}: it still has children.", request.CategoryId);

            return Result.Failure(CatalogErrors.CategoryHasChildren);
        }

        _categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} deleted.", request.CategoryId);

        return Result.Success();
    }
}
