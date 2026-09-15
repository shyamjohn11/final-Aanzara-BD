using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Permission : AuditableEntity
    {
        public Guid PermissionId { get; set; }

        public string PermissionName { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;
    }
}