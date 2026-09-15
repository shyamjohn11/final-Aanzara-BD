namespace ECommercePlatform.Domain.Entities
{
    public class PricingRequest : AdminEntity
    {
        public string CustomerName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string Product { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal RequestedPrice { get; set; }

        public string? Message { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
