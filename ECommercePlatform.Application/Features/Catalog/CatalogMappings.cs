using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Catalog;

/// <summary>
/// Entity-to-response mapping in one place. Hand-written rather than convention-
/// based so the wire contract only changes when someone edits this file.
/// </summary>
public static class CatalogMappings
{
    public static CategoryResponse ToResponse(this Category c) => new()
    {
        CategoryId = c.CategoryId,
        CategoryCode = c.CategoryCode,
        CategoryName = c.CategoryName,
        Description = c.Description,
        HasSubCategory = c.HasSubCategory,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    public static CategoryDetailResponse ToDetailResponse(
        this Category c, IEnumerable<CategoryImage> images) => new()
    {
        CategoryId = c.CategoryId,
        CategoryCode = c.CategoryCode,
        CategoryName = c.CategoryName,
        Description = c.Description,
        HasSubCategory = c.HasSubCategory,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Images = images.OrderBy(i => i.DisplayOrder).Select(ToResponse).ToArray()
    };

    public static CategoryImageResponse ToResponse(this CategoryImage i) => new()
    {
        CategoryImageId = i.CategoryImageId,
        CategoryId = i.CategoryId,
        ImageUrl = $"/api/v1/category-images/{i.CategoryImageId}/file",
        FileName = i.FileName,
        ContentType = i.ContentType,
        FileSize = i.FileSize,
        IsPrimary = i.IsPrimary,
        DisplayOrder = i.DisplayOrder,
        CreatedAt = i.CreatedAt
    };

    public static SubCategoryImageResponse ToResponse(this SubCategoryImage i) => new()
    {
        ImageId = i.ImageId,
        SubCategoryId = i.SubCategoryId,
        ImageUrl = i.ImageUrl,
        IsPrimary = i.IsPrimary,
        DisplayOrder = i.DisplayOrder,
        CreatedAt = i.CreatedAt
    };

    public static SubCategoryResponse ToResponse(this SubCategory s) => new()
    {
        SubCategoryId = s.SubCategoryId,
        SubCategoryCode = s.SubCategoryCode,
        SubCategoryName = s.SubCategoryName,
        Description = s.Description,
        CategoryId = s.CategoryId,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };

    public static ProductResponse ToResponse(this Product p) => new()
    {
        ProductId = p.ProductId,
        CategoryId = p.CategoryId,
        SubCategoryId = p.SubCategoryId,
        BrandId = p.BrandId,
        DealerId = p.DealerId,
        ProductName = p.ProductName,
        Sku = p.Sku,
        Description = p.Description,
        Specification = p.Specification,
        Price = p.Price,
        Mrp = p.Mrp,
        Discount = p.Discount,
        Moq = p.Moq,
        IsOrganic = p.IsOrganic,
        IsGstFree = p.IsGstFree,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    public static ProductSummaryResponse ToSummary(this Product p) => new()
    {
        ProductId = p.ProductId,
        Sku = p.Sku,
        ProductName = p.ProductName,
        CategoryId = p.CategoryId,
        SubCategoryId = p.SubCategoryId,
        BrandId = p.BrandId,
        DealerId = p.DealerId,
        Price = p.Price,
        Mrp = p.Mrp,
        Discount = p.Discount,
        Moq = p.Moq,
        Status = p.Status
    };

    public static BrandResponse ToResponse(this Brand b) => new()
    {
        BrandId = b.BrandId,
        BrandName = b.BrandName,
        Description = b.Description,
        IsOnSale = b.IsOnSale,
        Status = b.Status,
        // Stored URLs point at a host nothing serves; hand back the anonymous
        // streaming route instead (same pattern as category/sub-category images).
        ImageUrl = b.Images is null || b.Images.Count == 0
            ? null
            : $"/api/admin/brands/{b.BrandId}/image/file",
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}