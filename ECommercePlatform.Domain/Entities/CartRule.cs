namespace ECommercePlatform.Domain.Entities
{
    public class CartRule : AdminEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Condition { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public int Priority { get; set; }

        public DateTimeOffset? StartsAt { get; set; }

        public DateTimeOffset? EndsAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
