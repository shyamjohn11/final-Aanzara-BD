namespace ECommercePlatform.Domain.Entities
{
    public class Offer : AdminEntity
    {
        public string Title { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Code { get; set; }

        public string DiscountType { get; set; } = string.Empty;

        public decimal DiscountValue { get; set; }

        public decimal MinOrderValue { get; set; }

        public DateTimeOffset? StartsAt { get; set; }

        public DateTimeOffset? EndsAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
