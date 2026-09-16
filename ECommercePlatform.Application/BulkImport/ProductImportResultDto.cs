namespace ECommercePlatform.Application.BulkImport;

public sealed record ProductImportResultDto
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public ICollection<ProductImportErrorDto> Errors { get; set; } = new List<ProductImportErrorDto>();
}

public sealed record ProductImportErrorDto
{
    public int RowNumber { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}