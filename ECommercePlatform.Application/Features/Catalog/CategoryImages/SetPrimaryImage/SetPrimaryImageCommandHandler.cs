using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.SetPrimaryImage;

public sealed class SetPrimaryImageCommandHandler
    : ICommandHandler<SetPrimaryImageCommand, Result<CategoryImageResponse>>
{
    private readonly ICategoryImageRepository _images;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetPrimaryImageCommandHandler> _logger;

    public SetPrimaryImageCommandHandler(
        ICategoryImageRepository images,
        IUnitOfWork unitOfWork,
        ILogger<SetPrimaryImageCommandHandler> logger)
    {
        _images = images;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryImageResponse>> Handle(
        SetPrimaryImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _images.GetByIdAsync(request.CategoryImageId, cancellationToken);

        if (image is null)
        {
            return Result.Failure<CategoryImageResponse>(CatalogErrors.ImageNotFound);
        }

        // Demote the incumbent first: a filtered unique index allows only one
        // primary per category, so setting before clearing would fail.
        await _images.ClearPrimaryAsync(image.CategoryId, cancellationToken);

        image.IsPrimary = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Image {ImageId} is now primary for category {CategoryId}.",
            image.CategoryImageId, image.CategoryId);

        return Result.Success(image.ToResponse());
    }
}
