using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    /// <summary>
    /// Base for the admin CMS-style tables (banners, offers, reviews, ...).
    /// Single Guid primary key plus the standard audit columns.
    /// </summary>
    public class AdminEntity : AuditableEntity
    {
        public Guid Id { get; set; }
    }
}
