using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.GetProductImageFile;

public sealed class GetProductImageFileQueryHandler
    : IQueryHandler<GetProductImageFileQuery, Result<CategoryImageFileResponse>>
{
    private readonly IProductRepository _products;
    private readonly IProductImageRepository _images;
    private readonly IFileStorageService _storage;

    public GetProductImageFileQueryHandler(
        IProductRepository products,
        IProductImageRepository images,
        IFileStorageService storage)
    {
        _products = products;
        _images = images;
        _storage = storage;
    }

    public async Task<Result<CategoryImageFileResponse>> Handle(
        GetProductImageFileQuery request, CancellationToken cancellationToken)
    {
        if (await _products.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.ProductNotFound);
        }

        var images = await _images.GetForProductAsync(request.ProductId, cancellationToken);

        var image = images.FirstOrDefault(i => i.ImageId == request.ImageId);

        if (image is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.ImageFileMissing);
        }

        var path = _storage.TryResolvePath(image.ImageUrl);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.ImageFileMissing);
        }

        return Result.Success(new CategoryImageFileResponse(
            path,
            GetContentType(image.ImageUrl),
            GetFileName(image.ImageUrl)));
    }

    private static string GetContentType(string url)
    {
        var extension = Path.GetExtension(url).ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    private static string GetFileName(string url)
        => url.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "image";
}
