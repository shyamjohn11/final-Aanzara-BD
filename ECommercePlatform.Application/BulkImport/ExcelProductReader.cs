using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ECommercePlatform.Application.BulkImport;

namespace ECommercePlatform.Application.BulkImport;

public sealed class ExcelProductReader
{
    public async Task<ICollection<ExcelProductRowDto>> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var products = new Collection<ExcelProductRowDto>();

        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[1];

        var headers = new Collection<string>();
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            headers.Add(worksheet.Cells[1, col].Text ?? string.Empty);
        }

        int rowCount = 2;
        while (worksheet.Dimension.End.Row >= rowCount)
        {
            var dto = new ExcelProductRowDto();

            dto.Sku = ReadCell(worksheet, rowCount, headers.IndexOf("SKU") + 1) ?? string.Empty;
            dto.ProductName = ReadCell(worksheet, rowCount, headers.IndexOf("ProductName") + 1) ?? string.Empty;

            dto.Category = ReadCell(worksheet, rowCount, headers.IndexOf("Category") + 1);
            dto.SubCategory = ReadCell(worksheet, rowCount, headers.IndexOf("SubCategory") + 1);
            dto.Brand = ReadCell(worksheet, rowCount, headers.IndexOf("Brand") + 1);
            dto.Description = ReadCell(worksheet, rowCount, headers.IndexOf("Description") + 1);

            if (decimal.TryParse(ReadCell(worksheet, rowCount, headers.IndexOf("Price") + 1), out var price))
            {
                dto.Price = price;
            }

            if (decimal.TryParse(ReadCell(worksheet, rowCount, headers.IndexOf("GSTPercentage") + 1), out var gst))
            {
                dto.GstPercentage = gst;
            }

            if (int.TryParse(ReadCell(worksheet, rowCount, headers.IndexOf("Stock") + 1), out var stock))
            {
                dto.Stock = stock;
            }

            dto.Status = ReadCell(worksheet, rowCount, headers.IndexOf("Status") + 1) ?? "Active";

            products.Add(dto);
            rowCount++;
        }

        return products;
    }

    private static string? ReadCell(ExcelWorksheet worksheet, int row, int column)
    {
        if (column < 1 || column > worksheet.Dimension.End.Column)
        {
            return null;
        }

        var cellValue = worksheet.Cells[row, column].Text;
        return string.IsNullOrWhiteSpace(cellValue) ? null : cellValue;
    }
}