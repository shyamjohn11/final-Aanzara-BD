using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

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

    public async Task<ProductImportResultDto> ImportAsync(
        Stream fileStream, CancellationToken cancellationToken = default)
    {
        var result = new ProductImportResultDto();

        try
        {
            var reader = new ExcelProductReader();
            var rows = (await reader.ReadAsync(fileStream, cancellationToken)).ToList();

            result.TotalRows = rows.Count;

            var categories = await _categories.GetCategoriesWithSubCategoriesAsync(cancellationToken);
            var categoryByName = categories
                .GroupBy(c => c.CategoryName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var subCategories = (await _subCategories.SearchAsync(
                    null, null, null, 1, int.MaxValue, cancellationToken)).Items;

            var brands = (await _brands.SearchAsync(
                    null, null, 1, int.MaxValue, cancellationToken)).Items;
            var brandByName = brands
                .GroupBy(b => b.BrandName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var existingSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in (await _products.SearchAsync(
                         new ProductFilter(), 1, int.MaxValue, cancellationToken)).Items)
            {
                existingSkus.Add(product.Sku.Trim());
            }

            var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rowNumber = 1; // Header is row 1; data starts at row 2.

            foreach (var row in rows)
            {
                rowNumber++;

                if (IsBlankRow(row))
                {
                    result.TotalRows--;
                    continue;
                }

                var sku = (row.Sku ?? string.Empty).Trim();
                var name = (row.ProductName ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(sku))
                {
                    AddError(result, rowNumber, sku, "SKU", "SKU is required.");
                    continue;
                }

                if (existingSkus.Contains(sku) || !seenSkus.Add(sku))
                {
                    AddError(result, rowNumber, sku, "SKU", $"SKU '{sku}' already exists.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    AddError(result, rowNumber, sku, "ProductName", "Product name is required.");
                    continue;
                }

                // Supplier sheets carry "OUR MRP Price" as the selling price;
                // when it is missing, derive it from MRP Price + Discount %.
                var price = row.Price;
                if (price <= 0 && row.Mrp > 0 && row.Discount > 0 && row.Discount <= 100)
                {
                    price = Math.Round(row.Mrp * (1 - row.Discount / 100), 2);
                }

                if (price <= 0)
                {
                    AddError(result, rowNumber, sku, "Price", "Price must be greater than 0.");
                    continue;
                }

                var mrp = row.Mrp > 0 ? row.Mrp : price;

                if (row.Discount is < 0 or > 100)
                {
                    AddError(result, rowNumber, sku, "Discount", "Discount must be between 0 and 100.");
                    continue;
                }

                Guid? categoryId = null;
                if (!string.IsNullOrWhiteSpace(row.Category))
                {
                    if (!categoryByName.TryGetValue(row.Category.Trim(), out var category))
                    {
                        AddError(result, rowNumber, sku, "Category",
                            $"Category '{row.Category}' does not exist.");
                        continue;
                    }

                    categoryId = category.CategoryId;
                }

                Guid? subCategoryId = null;
                if (!string.IsNullOrWhiteSpace(row.SubCategory))
                {
                    var wanted = row.SubCategory.Trim();
                    var match = subCategories.FirstOrDefault(s =>
                        s.SubCategoryName.Equals(wanted, StringComparison.OrdinalIgnoreCase) &&
                        (categoryId is null || s.CategoryId == categoryId))
                        ?? subCategories.FirstOrDefault(s =>
                            s.SubCategoryName.Equals(wanted, StringComparison.OrdinalIgnoreCase));

                    if (match is null)
                    {
                        AddError(result, rowNumber, sku, "SubCategory",
                            $"Sub-category '{row.SubCategory}' does not exist.");
                        continue;
                    }

                    if (categoryId is not null && match.CategoryId != categoryId)
                    {
                        AddError(result, rowNumber, sku, "SubCategory",
                            $"Sub-category '{row.SubCategory}' does not belong to category '{row.Category}'.");
                        continue;
                    }

                    subCategoryId = match.SubCategoryId;
                    categoryId ??= match.CategoryId;
                }

                Guid? brandId = null;
                if (!string.IsNullOrWhiteSpace(row.Brand))
                {
                    if (!brandByName.TryGetValue(row.Brand.Trim(), out var brand))
                    {
                        AddError(result, rowNumber, sku, "Brand",
                            $"Brand '{row.Brand}' does not exist.");
                        continue;
                    }

                    brandId = brand.BrandId;
                }

                var status = string.IsNullOrWhiteSpace(row.Status)
                    ? ProductStatus.Active
                    : row.Status.Trim();

                if (!ProductStatus.IsValid(status))
                {
                    AddError(result, rowNumber, sku, "Status",
                        "Status must be 'Active' or 'Inactive'.");
                    continue;
                }

                status = status.Equals(ProductStatus.Inactive, StringComparison.OrdinalIgnoreCase)
                    ? ProductStatus.Inactive
                    : ProductStatus.Active;

                _products.Add(new Product
                {
                    ProductId = Guid.NewGuid(),
                    CategoryId = categoryId,
                    SubCategoryId = subCategoryId,
                    BrandId = brandId,
                    ProductName = name,
                    Sku = sku,
                    Description = string.IsNullOrWhiteSpace(row.Description)
                        ? null
                        : row.Description.Trim(),
                    Price = price,
                    Mrp = mrp,
                    Discount = row.Discount,
                    Moq = row.Moq > 0 ? row.Moq : 1,
                    IsOrganic = false,
                    IsGstFree = false,
                    Status = status
                });

                result.SuccessfulRows++;
            }

            if (result.SuccessfulRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            result.FailedRows = result.TotalRows - result.SuccessfulRows;
            result.Success = result.TotalRows > 0 && result.FailedRows == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk product import");
            result.Success = false;
            result.Errors.Add(new ProductImportErrorDto
            {
                RowNumber = 0,
                Sku = string.Empty,
                Field = "File",
                Message = ex is InvalidOperationException
                    ? ex.Message
                    : "Could not read the Excel file. Make sure it is a valid single-sheet .xlsx file using the template columns."
            });
        }

        return result;
    }

    public async Task<byte[]> ExportAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categories.GetCategoriesWithSubCategoriesAsync(cancellationToken);
        var categoryNames = categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);

        var subCategories = (await _subCategories.SearchAsync(
                null, null, null, 1, int.MaxValue, cancellationToken)).Items;
        var subCategoryNames = subCategories.ToDictionary(s => s.SubCategoryId, s => s.SubCategoryName);

        var brands = (await _brands.SearchAsync(
                null, null, 1, int.MaxValue, cancellationToken)).Items;
        var brandNames = brands.ToDictionary(b => b.BrandId, b => b.BrandName);

        var products = (await _products.SearchAsync(
                new ProductFilter(), 1, int.MaxValue, cancellationToken)).Items;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Products");

        string[] headers =
        [
            "SKU", "ProductName", "Category", "SubCategory", "Brand",
            "Description", "Price", "GSTPercentage", "Stock", "Status"
        ];

        for (var col = 0; col < headers.Length; col++)
        {
            worksheet.Cells[1, col + 1].Value = headers[col];
            worksheet.Column(col + 1).Width = col is 1 or 5 ? 30 : 20;
        }

        using var range = worksheet.Cells[1, 1, 1, headers.Length];
        range.Style.Font.Bold = true;

        var excelRow = 2;
        foreach (var product in products.OrderBy(p => p.ProductName))
        {
            worksheet.Cells[excelRow, 1].Value = product.Sku;
            worksheet.Cells[excelRow, 2].Value = product.ProductName;
            worksheet.Cells[excelRow, 3].Value = product.CategoryId is not null &&
                categoryNames.TryGetValue(product.CategoryId.Value, out var categoryName)
                    ? categoryName
                    : string.Empty;
            worksheet.Cells[excelRow, 4].Value = product.SubCategoryId is not null &&
                subCategoryNames.TryGetValue(product.SubCategoryId.Value, out var subCategoryName)
                    ? subCategoryName
                    : string.Empty;
            worksheet.Cells[excelRow, 5].Value = product.BrandId is not null &&
                brandNames.TryGetValue(product.BrandId.Value, out var brandName)
                    ? brandName
                    : string.Empty;
            worksheet.Cells[excelRow, 6].Value = product.Description ?? string.Empty;
            worksheet.Cells[excelRow, 7].Value = product.Price;
            worksheet.Cells[excelRow, 8].Value = string.Empty;
            worksheet.Cells[excelRow, 9].Value = string.Empty;
            worksheet.Cells[excelRow, 10].Value = product.Status;
            excelRow++;
        }

        return package.GetAsByteArray();
    }

    private static bool IsBlankRow(ExcelProductRowDto row) =>
        string.IsNullOrWhiteSpace(row.Sku) &&
        string.IsNullOrWhiteSpace(row.ProductName) &&
        string.IsNullOrWhiteSpace(row.Category) &&
        string.IsNullOrWhiteSpace(row.SubCategory) &&
        string.IsNullOrWhiteSpace(row.Brand) &&
        string.IsNullOrWhiteSpace(row.Description) &&
        row.Price == 0 &&
        row.Mrp == 0;

    private static void AddError(
        ProductImportResultDto result, int rowNumber, string sku, string field, string message)
    {
        result.Errors.Add(new ProductImportErrorDto
        {
            RowNumber = rowNumber,
            Sku = sku,
            Field = field,
            Message = message
        });
    }
}
