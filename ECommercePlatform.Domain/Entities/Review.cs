namespace ECommercePlatform.Domain.Entities
{
    public class Review : AdminEntity
    {
        public string ProductName { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
