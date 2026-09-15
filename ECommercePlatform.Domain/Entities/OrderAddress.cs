using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class OrderAddress : AuditableEntity
    {
        public Guid OrderAddressId { get; set; }

        public Guid OrderId { get; set; }

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientPhone { get; set; } = string.Empty;

        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Pincode { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public Order Order { get; set; } = null!;
    }
}