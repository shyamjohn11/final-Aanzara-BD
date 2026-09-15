using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Invoice : AuditableEntity
    {
        public Guid InvoiceId { get; set; }

        public Guid OrderId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public string? GstBreakdownJson { get; set; }

        public string? PdfUrl { get; set; }

        public DateTime GeneratedAt { get; set; }

        public Order Order { get; set; } = null!;
    }
}