using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.SetPrimaryProductImage;

public sealed class SetPrimaryProductImageCommandHandler
    : ICommandHandler<SetPrimaryProductImageCommand, Result<ProductImageResponse>>
{
    private readonly IProductImageRepository _images;
    private readonly IUnitOfWork _unitOfWork;

    public SetPrimaryProductImageCommandHandler(
        IProductImageRepository images, IUnitOfWork unitOfWork)
    {
        _images = images;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductImageResponse>> Handle(
        SetPrimaryProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _images.GetByIdAsync(request.ImageId, cancellationToken);

        if (image is null || image.ProductId != request.ProductId)
        {
            return Result.Failure<ProductImageResponse>(Error.NotFound(
                "catalog.product_image_not_found", "The product image could not be found."));
        }

        await _images.ClearPrimaryAsync(request.ProductId, cancellationToken);
        image.IsPrimary = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(image.ToResponse());
    }
}
