using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.DeleteProductImage;

public sealed class DeleteProductImageCommandHandler
    : ICommandHandler<DeleteProductImageCommand, Result>
{
    private readonly IProductImageRepository _images;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductImageCommandHandler(
        IProductImageRepository images,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _images = images;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _images.GetByIdAsync(request.ImageId, cancellationToken);

        if (image is null || image.ProductId != request.ProductId)
        {
            return Result.Failure(Error.NotFound(
                "catalog.product_image_not_found", "The product image could not be found."));
        }

        var wasPrimary = image.IsPrimary;
        var imageUrl = image.ImageUrl;

        _images.Remove(image);

        if (wasPrimary)
        {
            // Promote the next image so listings keep showing something.
            var remaining = await _images.GetForProductAsync(request.ProductId, cancellationToken);
            var next = remaining
                .Where(i => i.ImageId != request.ImageId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefault();

            if (next is not null)
            {
                next.IsPrimary = true;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _fileStorage.DeleteByUrlAsync(imageUrl, cancellationToken);

        return Result.Success();
    }
}
