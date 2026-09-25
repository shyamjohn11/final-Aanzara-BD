namespace ECommercePlatform.Domain.Entities
{
    public class Review : AdminEntity
    {
        public string ProductName { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Author account when the review came from the storefront submit
        /// flow (null for admin-entered rows). Used to prove verified
        /// purchase without trusting a client-sent name.
        /// </summary>
        public Guid? UserId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        /// <summary>
        /// True only when the author was verified as a buyer of the product
        /// at submit time. Drives the storefront "Verified Buyer" badge.
        /// </summary>
        public bool IsVerifiedPurchase { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
