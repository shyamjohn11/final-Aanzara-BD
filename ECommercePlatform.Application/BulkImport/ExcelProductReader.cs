using OfficeOpenXml;

namespace ECommercePlatform.Application.BulkImport;

public sealed class ExcelProductReader
{
    // Canonical field -> accepted header spellings (normalized: upper-case,
    // letters/digits only). Covers both the import template
    // (SKU/ProductName/Category/...) and supplier price-list sheets such as:
    //   PRODUCT NUMBER | Hair colour ITEMS | Qty | MRP Price | Discount % |
    //   Other Tax | OUR MRP Price | Customer Profit
    // Mapping used for supplier sheets:
    //   PRODUCT NUMBER -> Sku, *ITEMS* -> ProductName, Qty -> Moq,
    //   MRP Price -> Mrp, Discount % -> Discount, OUR MRP Price -> Price.
    // Columns with no product field (Other Tax, Customer Profit) are ignored.
    private static readonly IReadOnlyDictionary<string, string> HeaderAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SKU"] = "SKU",
            ["PRODUCTNUMBER"] = "SKU",
            ["PRODUCTNO"] = "SKU",
            ["ITEMCODE"] = "SKU",
            ["PRODUCTNAME"] = "ProductName",
            ["CATEGORY"] = "Category",
            ["SUBCATEGORY"] = "SubCategory",
            ["BRAND"] = "Brand",
            ["DESCRIPTION"] = "Description",
            ["PRICE"] = "Price",
            ["OURMRPPRICE"] = "Price",
            ["OURPRICE"] = "Price",
            ["SELLINGPRICE"] = "Price",
            ["MRPPRICE"] = "Mrp",
            ["MRP"] = "Mrp",
            ["DISCOUNT"] = "Discount",
            ["DISCOUNTPERCENT"] = "Discount",
            ["QTY"] = "Moq",
            ["QUANTITY"] = "Moq",
            ["MOQ"] = "Moq",
            ["GSTPERCENTAGE"] = "GSTPercentage",
            ["GST"] = "GSTPercentage",
            ["STOCK"] = "Stock",
            ["STATUS"] = "Status",
        };

    public Task<ICollection<ExcelProductRowDto>> ReadAsync(
        Stream stream, CancellationToken cancellationToken = default)
    {
        var products = new List<ExcelProductRowDto>();

        using var package = new ExcelPackage(stream);

        // NOTE: EPPlus worksheets are 0-indexed. Index 1 is the *second*
        // sheet, so single-sheet template/export files threw here and every
        // upload silently returned zero rows.
        if (package.Workbook.Worksheets.Count == 0)
        {
            return Task.FromResult<ICollection<ExcelProductRowDto>>(products);
        }

        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet.Dimension is null)
        {
            return Task.FromResult<ICollection<ExcelProductRowDto>>(products);
        }

        // Supplier files often have title/blank rows above the table, so the
        // header row is located by scanning (not assumed to be row 1).
        var headerRow = FindHeaderRow(worksheet);
        if (headerRow is null)
        {
            throw new InvalidOperationException(
                "No header row found. The first sheet must contain an SKU column " +
                "('SKU' or 'PRODUCT NUMBER') within the first 20 rows.");
        }

        var columnsByHeader = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var rawHeaders = new Dictionary<int, string>();
        for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var normalized = NormalizeHeader(worksheet.Cells[headerRow.Value, col].Text);
            if (string.IsNullOrEmpty(normalized))
            {
                continue;
            }

            rawHeaders[col] = normalized;
            if (HeaderAliases.TryGetValue(normalized, out var canonical) &&
                !columnsByHeader.ContainsKey(canonical))
            {
                columnsByHeader[canonical] = col;
            }
        }

        // Supplier sheets name the item column freely ("Hair colour ITEMS",
        // "ITEMS", "Item Name", ...): fall back to the first header
        // containing "ITEM" when no exact ProductName header matched.
        if (!columnsByHeader.ContainsKey("ProductName"))
        {
            foreach (var (col, normalized) in rawHeaders)
            {
                if (normalized.Contains("ITEM", StringComparison.OrdinalIgnoreCase))
                {
                    columnsByHeader["ProductName"] = col;
                    break;
                }
            }
        }

        for (var row = headerRow.Value + 1; row <= worksheet.Dimension.End.Row; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dto = new ExcelProductRowDto
            {
                Sku = ReadCell(worksheet, row, columnsByHeader, "SKU") ?? string.Empty,
                ProductName = ReadCell(worksheet, row, columnsByHeader, "ProductName") ?? string.Empty,
                Category = ReadCell(worksheet, row, columnsByHeader, "Category"),
                SubCategory = ReadCell(worksheet, row, columnsByHeader, "SubCategory"),
                Brand = ReadCell(worksheet, row, columnsByHeader, "Brand"),
                Description = ReadCell(worksheet, row, columnsByHeader, "Description"),
                Status = ReadCell(worksheet, row, columnsByHeader, "Status") ?? "Active",
            };

            if (ParseDecimal(ReadCell(worksheet, row, columnsByHeader, "Price"), out var price))
            {
                dto.Price = price;
            }

            if (ParseDecimal(ReadCell(worksheet, row, columnsByHeader, "Mrp"), out var mrp))
            {
                dto.Mrp = mrp;
            }

            if (ParseDecimal(ReadCell(worksheet, row, columnsByHeader, "Discount"), out var discount))
            {
                dto.Discount = discount;
            }

            if (int.TryParse(ReadCell(worksheet, row, columnsByHeader, "Moq"), out var moq))
            {
                dto.Moq = moq;
            }

            if (ParseDecimal(ReadCell(worksheet, row, columnsByHeader, "GSTPercentage"), out var gst))
            {
                dto.GstPercentage = gst;
            }

            if (int.TryParse(ReadCell(worksheet, row, columnsByHeader, "Stock"), out var stock))
            {
                dto.Stock = stock;
            }

            products.Add(dto);
        }

        return Task.FromResult<ICollection<ExcelProductRowDto>>(products);
    }

    /// <summary>
    /// Finds the header row: the first row (within the first 20) containing
    /// a recognized SKU header ('SKU', 'PRODUCT NUMBER', ...). Returns null
    /// when the sheet has no identifiable header row.
    /// </summary>
    private static int? FindHeaderRow(ExcelWorksheet worksheet)
    {
        var lastRow = Math.Min(worksheet.Dimension.End.Row, 20);
        for (var row = 1; row <= lastRow; row++)
        {
            for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var normalized = NormalizeHeader(worksheet.Cells[row, col].Text);
                if (string.IsNullOrEmpty(normalized))
                {
                    continue;
                }

                if (HeaderAliases.TryGetValue(normalized, out var canonical) &&
                    canonical == "SKU")
                {
                    return row;
                }
            }
        }

        return null;
    }

    private static string NormalizeHeader(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        return new string(header
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }

    /// <summary>
    /// Parses numbers as well as percent strings ("50%", "50 %").
    /// A fractional value with a stray % sign ("0.5%") is scaled to 50.
    /// </summary>
    private static bool ParseDecimal(string? text, out decimal value)
    {
        value = 0;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var cleaned = text.Trim();
        var isPercent = false;

        if (cleaned.EndsWith("%", StringComparison.Ordinal))
        {
            isPercent = true;
            cleaned = cleaned[..^1].Trim();
        }

        if (!decimal.TryParse(cleaned, out value))
        {
            return false;
        }

        if (isPercent && value > 0 && value < 1)
        {
            value *= 100;
        }

        return true;
    }

    private static string? ReadCell(
        ExcelWorksheet worksheet,
        int row,
        IReadOnlyDictionary<string, int> columnsByHeader,
        string header)
    {
        if (!columnsByHeader.TryGetValue(header, out var column))
        {
            return null;
        }

        var cellValue = worksheet.Cells[row, column].Text;
        return string.IsNullOrWhiteSpace(cellValue) ? null : cellValue.Trim();
    }
}
