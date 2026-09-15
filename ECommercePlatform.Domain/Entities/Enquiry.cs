namespace ECommercePlatform.Domain.Entities
{
    public class Enquiry : AdminEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string? Message { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
