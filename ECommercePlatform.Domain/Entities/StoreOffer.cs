namespace ECommercePlatform.Domain.Entities
{
    public class StoreOffer : AdminEntity
    {
        public Guid StoreId { get; set; }

        public string StoreName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal DiscountValue { get; set; }

        public DateTimeOffset? StartsAt { get; set; }

        public DateTimeOffset? EndsAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
