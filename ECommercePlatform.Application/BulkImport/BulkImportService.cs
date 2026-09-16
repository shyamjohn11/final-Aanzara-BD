using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Application.BulkImport;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace ECommercePlatform.Application.BulkImport;

public sealed class BulkImportService : IBulkImportService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly ISubCategoryRepository _subCategories;
    private readonly IBrandRepository _brands;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkImportService> _logger;

    public BulkImportService(
        IProductRepository products,
        ICategoryRepository categories,
        ISubCategoryRepository subCategories,
        IBrandRepository brands,
        IUnitOfWork unitOfWork,
        ILogger<BulkImportService> logger)
    {
        _products = products;
        _categories = categories;
        _subCategories = subCategories;
        _brands = brands;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProductImportResultDto> ImportAsync(Stream fileStream)
    {
        var result = new ProductImportResultDto();

        try
        {
            var reader = new ExcelProductReader();
            var rows = await reader.ReadAsync(fileStream, default);

            result.TotalRows = rows.Count;

            var validCategories = await _categories.GetCategoriesWithSubCategoriesAsync(default);
            var validCategoryNames = validCategories.Select(c => c.CategoryName).ToHashSet();
            var validBrandNames = (await _brands.SearchAsync(null, null, 1, int.MaxValue, default)).Items
                .Select(b => b.BrandName).ToHashSet();

            var existingSkus = new HashSet<string>();
            foreach (var product in (await _products.SearchAsync(new ProductFilter(), 1, int.MaxValue, default)).Items)
            {
                existingSkus.Add(product.Sku);
            }

            foreach (var row in rows)
            {
                var hasError = false;

                if (string.IsNullOrWhiteSpace(row.Sku) || existingSkus.Contains(row.Sku, StringComparer.OrdinalIgnoreCase))
                {
                    hasError = true;
                }
                else if (string.IsNullOrWhiteSpace(row.ProductName))
                {
                    hasError = true;
                }
                else if (row.Category is not null && !validCategoryNames.Contains(row.Category))
                {
                    hasError = true;
                }
                else if (row.Brand is not null && !validBrandNames.Contains(row.Brand))
                {
                    hasError = true;
                }

                if (!hasError)
                {
                    result.SuccessfulRows++;
                }
                else
                {
                    result.FailedRows++;
                    result.Errors.Add(new ProductImportErrorDto
                    {
                        RowNumber = 0,
                        Sku = row.Sku,
                        Field = "Validation",
                        Message = "Invalid product data"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk product import");
        }

        return result;
    }
}