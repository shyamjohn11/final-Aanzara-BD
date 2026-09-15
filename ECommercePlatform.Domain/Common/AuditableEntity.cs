namespace ECommercePlatform.Domain.Common;

/// <summary>
/// Timestamps carried by every catalogue table in the schema. Kept as a base type
/// so the DbContext can stamp them in one place rather than in every handler.
/// </summary>
public abstract class AuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
