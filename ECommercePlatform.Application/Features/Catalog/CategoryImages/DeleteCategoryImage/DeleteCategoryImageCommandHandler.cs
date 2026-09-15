using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.DeleteCategoryImage;

public sealed class DeleteCategoryImageCommandHandler
    : ICommandHandler<DeleteCategoryImageCommand, Result>
{
    private readonly ICategoryImageRepository _images;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCategoryImageCommandHandler> _logger;

    public DeleteCategoryImageCommandHandler(
        ICategoryImageRepository images,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCategoryImageCommandHandler> logger)
    {
        _images = images;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteCategoryImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _images.GetByIdAsync(request.CategoryImageId, cancellationToken);

        if (image is null)
        {
            return Result.Failure(CatalogErrors.ImageNotFound);
        }

        var wasPrimary = image.IsPrimary;
        var categoryId = image.CategoryId;

        _images.Remove(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Deleting the primary would otherwise leave the category with images but
        // none flagged, so promote the next one in display order.
        if (wasPrimary)
        {
            var remaining = await _images.GetForCategoryAsync(categoryId, cancellationToken);
            var successor = remaining.OrderBy(i => i.DisplayOrder).FirstOrDefault();

            if (successor is not null)
            {
                successor.IsPrimary = true;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Image {ImageId} promoted to primary for category {CategoryId}.",
                    successor.CategoryImageId, categoryId);
            }
        }

        _logger.LogInformation("Image {ImageId} deleted.", request.CategoryImageId);

        return Result.Success();
    }
}
