namespace ECommercePlatform.Domain.Entities
{
    public class Banner : AdminEntity
    {
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        public string? ImageUrl { get; set; }

        public string? Link { get; set; }

        public string Position { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }

        public int Clicks { get; set; }
    }
}
