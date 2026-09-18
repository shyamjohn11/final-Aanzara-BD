using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    /// <summary>
    /// A dealer/shop owned by a customer and served by one agent
    /// (Agent 1 ── * Dealer). Products optionally hang off a dealer via
    /// Product.DealerId; legacy/global products keep DealerId null.
    /// </summary>
    public class Dealer : AdminEntity
    {
        public Guid AgentId { get; set; }

        public string DealerCode { get; set; } = string.Empty;

        public string ShopName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string? AlternatePhone { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        public string? Pincode { get; set; }

        public string? GSTNumber { get; set; }

        public string? PANNumber { get; set; }

        public string? ShopDescription { get; set; }

        public string? ShopLogo { get; set; }

        public DealerStatus Status { get; set; } = DealerStatus.Active;

        public Agent Agent { get; set; } = null!;
    }
}
