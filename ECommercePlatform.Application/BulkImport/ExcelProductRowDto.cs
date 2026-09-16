namespace ECommercePlatform.Application.BulkImport;

public sealed record ExcelProductRowDto
{
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal GstPercentage { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = "Active";
}