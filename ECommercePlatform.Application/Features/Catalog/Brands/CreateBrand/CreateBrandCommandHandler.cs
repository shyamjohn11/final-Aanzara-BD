using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Brands.CreateBrand;

public sealed class CreateBrandCommandHandler
    : ICommandHandler<CreateBrandCommand, Result<BrandResponse>>
{
    private const string ImageSubFolder = "Brands";

    private readonly IBrandRepository _brands;
    private readonly IBrandImageRepository _images;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBrandCommandHandler> _logger;

    public CreateBrandCommandHandler(
        IBrandRepository brands,
        IBrandImageRepository images,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<CreateBrandCommandHandler> logger)
    {
        _brands = brands;
        _images = images;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BrandResponse>> Handle(
        CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var name = request.BrandName.Trim();

        if (await _brands.NameExistsAsync(name, excludingId: null, cancellationToken))
        {
            return Result.Failure<BrandResponse>(CatalogErrors.BrandNameTaken(name));
        }

        var brand = new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = name,
            Description = request.Description?.Trim(),
            IsOnSale = request.IsOnSale,
            Status = request.Status
        };

        _brands.Add(brand);

        // Saved now so the brand has a row for the image to attach to below.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Image is not null)
        {
            var stored = await _fileStorage.SaveAsync(request.Image, ImageSubFolder, cancellationToken);

            var image = new BrandImage
            {
                BrandImageId = Guid.NewGuid(),
                BrandId = brand.BrandId,
                ImageUrl = stored.Url,
                IsPrimary = true
            };

            _images.Add(image);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            brand.Images.Add(image);
        }

        _logger.LogInformation("Brand {BrandId} ({BrandName}) created.", brand.BrandId, name);

        return Result.Success(brand.ToResponse());
    }
}