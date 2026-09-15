using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class BusinessAccount : AuditableEntity
    {
        public Guid BusinessAccountId { get; set; }

        public Guid UserId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string ShopName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string? GstNumber { get; set; }

        public string? DocumentUrl { get; set; }

        public VerificationStatus VerificationStatus { get; set; }

        public decimal CreditLimit { get; set; }

        public decimal CurrentBalance { get; set; }

        public User User { get; set; } = null!;
    }
}