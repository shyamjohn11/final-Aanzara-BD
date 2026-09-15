using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Brands.DeleteBrand;

public sealed class DeleteBrandCommandHandler : ICommandHandler<DeleteBrandCommand, Result>
{
    private readonly IBrandRepository _brands;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBrandCommandHandler> _logger;

    public DeleteBrandCommandHandler(
        IBrandRepository brands,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<DeleteBrandCommandHandler> logger)
    {
        _brands = brands;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _brands.GetWithImagesAsync(request.BrandId, cancellationToken);

        if (brand is null)
        {
            return Result.Failure(CatalogErrors.BrandNotFound);
        }

        // Refuse rather than silently orphan products that still reference this brand.
        if (await _brands.HasProductsAsync(request.BrandId, cancellationToken))
        {
            _logger.LogInformation(
                "Refused to delete brand {BrandId}: it still has products assigned.", request.BrandId);

            return Result.Failure(CatalogErrors.BrandHasProducts);
        }

        var imageUrls = brand.Images
            .Select(i => i.ImageUrl)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList();

        _brands.Remove(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var imageUrl in imageUrls)
        {
            try
            {
                await _fileStorage.DeleteByUrlAsync(imageUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete brand image file at {ImageUrl}.", imageUrl);
            }
        }

        _logger.LogInformation("Brand {BrandId} deleted.", request.BrandId);

        return Result.Success();
    }
}