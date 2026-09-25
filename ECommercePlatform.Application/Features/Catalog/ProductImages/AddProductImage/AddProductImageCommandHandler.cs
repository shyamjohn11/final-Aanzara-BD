using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Validation;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.AddProductImage;

public sealed class AddProductImageCommandHandler
    : ICommandHandler<AddProductImageCommand, Result<ProductImageResponse>>
{
    private const string ImageSubFolder = "Products";

    private readonly IProductRepository _products;
    private readonly IProductImageRepository _images;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddProductImageCommandHandler> _logger;

    public AddProductImageCommandHandler(
        IProductRepository products,
        IProductImageRepository images,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<AddProductImageCommandHandler> logger)
    {
        _products = products;
        _images = images;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductImageResponse>> Handle(
        AddProductImageCommand request, CancellationToken cancellationToken)
    {
        if (await _products.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            return Result.Failure<ProductImageResponse>(CatalogErrors.ProductNotFound);
        }

        if (request.File is null || request.File.Length <= 0)
        {
            return Result.Failure<ProductImageResponse>(Error.Validation(
                "catalog.product_image_required", "An image file is required."));
        }

        var maxBytes = 5L * 1024 * 1024;
        var validationError = await ImageFileValidator.ValidateAsync(
            request.File.Content,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            maxBytes,
            cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<ProductImageResponse>(Error.Validation(
                "catalog.product_image_invalid_type", validationError));
        }

        var existing = await _images.GetForProductAsync(request.ProductId, cancellationToken);

        // The first image becomes primary automatically — a product with images
        // but no primary would leave listings with nothing to show.
        var isPrimary = request.IsPrimary || existing.Count == 0;

        if (isPrimary)
        {
            await _images.ClearPrimaryAsync(request.ProductId, cancellationToken);
        }

        var stored = await _fileStorage.SaveAsync(request.File, ImageSubFolder, cancellationToken);

        var image = new ProductImage
        {
            ImageId = Guid.NewGuid(),
            ProductId = request.ProductId,
            ImageUrl = stored.Url,
            IsPrimary = isPrimary,
            DisplayOrder = await _images.NextDisplayOrderAsync(request.ProductId, cancellationToken)
        };

        _images.Add(image);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // A failed save must never leave an orphan file behind.
            await _fileStorage.DeleteByUrlAsync(stored.Url, cancellationToken);
            throw;
        }

        _logger.LogInformation(
            "Image {ImageId} added to product {ProductId}.", image.ImageId, request.ProductId);

        return Result.Success(image.ToResponse());
    }
}
