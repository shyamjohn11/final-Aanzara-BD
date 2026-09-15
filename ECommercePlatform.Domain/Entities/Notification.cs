namespace ECommercePlatform.Domain.Entities
{
    public class Notification : AdminEntity
    {
        public string Title { get; set; } = string.Empty;

        public string? Message { get; set; }

        public bool IsRead { get; set; }

        /// <summary>Machine-readable kind: order, payment, enquiry, pricing,
        /// quote, review, inventory, account, system.</summary>
        public string Type { get; set; } = "system";

        /// <summary>Deep link for the admin console, e.g. /admin/orders.</summary>
        public string? Link { get; set; }
    }
}
