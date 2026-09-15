namespace ECommercePlatform.Domain.Entities
{
    public class Combo : AdminEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public List<Guid> ProductIds { get; set; } = new();

        public decimal Price { get; set; }

        public decimal OriginalPrice { get; set; }

        public string? ImageUrl { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
