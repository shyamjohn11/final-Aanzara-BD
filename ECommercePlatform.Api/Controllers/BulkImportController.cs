using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.BulkImport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace ECommercePlatform.Api.Controllers;

[Route("api/admin/products")]
[ApiController]
[HasPermission("Product.Create")]
public sealed class BulkImportController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly IBulkImportService _bulkImportService;

    public BulkImportController(ISender sender, IBulkImportService bulkImportService)
        : base(sender)
    {
        _sender = sender;
        _bulkImportService = bulkImportService;
    }

    /// <summary>
    /// Download Excel template for bulk product import
    /// </summary>
    [HttpGet("bulk-import/template")]
    public IActionResult DownloadTemplate()
    {
        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Products");

        worksheet.Cells[1, 1].Value = "SKU";
        worksheet.Cells[1, 2].Value = "ProductName";
        worksheet.Cells[1, 3].Value = "Category";
        worksheet.Cells[1, 4].Value = "SubCategory";
        worksheet.Cells[1, 5].Value = "Brand";
        worksheet.Cells[1, 6].Value = "Description";
        worksheet.Cells[1, 7].Value = "Price";
        worksheet.Cells[1, 8].Value = "GSTPercentage";
        worksheet.Cells[1, 9].Value = "Stock";
        worksheet.Cells[1, 10].Value = "Status";

        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 30;
        worksheet.Column(3).Width = 20;
        worksheet.Column(4).Width = 20;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 30;
        worksheet.Column(7).Width = 15;
        worksheet.Column(8).Width = 15;
        worksheet.Column(9).Width = 15;
        worksheet.Column(10).Width = 15;

        using var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ProductImportTemplate.xlsx"
        );
    }

    /// <summary>
    /// Export every product to an Excel file (same columns as the template)
    /// </summary>
    [HttpGet("bulk-import/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAsync(CancellationToken cancellationToken)
    {
        var fileBytes = await _bulkImportService.ExportAsync(cancellationToken);

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ProductsExport-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx"
        );
    }

    /// <summary>
    /// Bulk import products from Excel file
    /// </summary>
    [HttpPost("bulk-import")]
    [ProducesResponseType(typeof(ProductImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductImportResultDto>> ImportAsync(
        [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please select an Excel file.");
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (fileExtension != ".xlsx")
        {
            return BadRequest("Only .xlsx files are supported.");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest("File size must be 10 MB or less.");
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        var result = await _bulkImportService.ImportAsync(stream, cancellationToken);

        return Ok(result);
    }
}