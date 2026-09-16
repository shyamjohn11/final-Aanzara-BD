namespace ECommercePlatform.Application.BulkImport;

public interface IBulkImportService
{
    Task<ProductImportResultDto> ImportAsync(Stream fileStream);
}