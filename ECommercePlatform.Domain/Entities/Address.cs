using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Address : AuditableEntity
    {
        public Guid AddressId { get; set; }

        public Guid UserId { get; set; }

        public string? Label { get; set; }

        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Pincode { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public bool IsDefault { get; set; }

        public User User { get; set; } = null!;
    }
}