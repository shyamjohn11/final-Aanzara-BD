namespace ECommercePlatform.Application.BulkImport;

public interface IBulkImportService
{
    Task<ProductImportResultDto> ImportAsync(
        Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports every product to an .xlsx workbook (same columns as the
    /// import template) and returns the file bytes.
    /// </summary>
    Task<byte[]> ExportAsync(CancellationToken cancellationToken = default);
}