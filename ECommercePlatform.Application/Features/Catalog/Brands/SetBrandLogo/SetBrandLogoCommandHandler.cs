using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Validation;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Brands.SetBrandLogo;

public sealed class SetBrandLogoCommandHandler
    : ICommandHandler<SetBrandLogoCommand, Result<BrandResponse>>
{
    private const string ImageSubFolder = "Brands";

    private readonly IBrandRepository _brands;
    private readonly IBrandImageRepository _images;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetBrandLogoCommandHandler> _logger;

    public SetBrandLogoCommandHandler(
        IBrandRepository brands,
        IBrandImageRepository images,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<SetBrandLogoCommandHandler> logger)
    {
        _brands = brands;
        _images = images;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BrandResponse>> Handle(
        SetBrandLogoCommand request, CancellationToken cancellationToken)
    {
        var brand = await _brands.GetWithImagesAsync(request.BrandId, cancellationToken);

        if (brand is null)
        {
            return Result.Failure<BrandResponse>(CatalogErrors.BrandNotFound);
        }

        if (request.File is null || request.File.Length <= 0)
        {
            return Result.Failure<BrandResponse>(Error.Validation(
                "catalog.brand_image_required", "An image file is required."));
        }

        var validationError = await ImageFileValidator.ValidateAsync(
            request.File.Content,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            5L * 1024 * 1024,
            cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<BrandResponse>(Error.Validation(
                "catalog.brand_image_invalid_type", validationError));
        }

        var stored = await _fileStorage.SaveAsync(request.File, ImageSubFolder, cancellationToken);
        var existingImage = brand.Images.FirstOrDefault(i => i.IsPrimary)
            ?? brand.Images.FirstOrDefault();
        var oldImageUrl = existingImage?.ImageUrl;

        if (existingImage is not null)
        {
            // Same row, new file: keeps BrandImageId stable for anything
            // already referencing it.
            existingImage.ImageUrl = stored.Url;
            existingImage.IsPrimary = true;
        }
        else
        {
            var newImage = new BrandImage
            {
                BrandImageId = Guid.NewGuid(),
                BrandId = brand.BrandId,
                ImageUrl = stored.Url,
                IsPrimary = true
            };

            _images.Add(newImage);
            brand.Images.Add(newImage);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Delete the newly uploaded file if the DB save fails, so a
            // failed update never leaves an orphan file behind.
            await _fileStorage.DeleteByUrlAsync(stored.Url, cancellationToken);
            throw;
        }

        // Only remove the old file from storage once the DB update commits.
        if (!string.IsNullOrWhiteSpace(oldImageUrl) && oldImageUrl != stored.Url)
        {
            await _fileStorage.DeleteByUrlAsync(oldImageUrl, cancellationToken);
        }

        _logger.LogInformation("Brand {BrandId} logo replaced.", brand.BrandId);

        return Result.Success(brand.ToResponse());
    }
}
