using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Role : AuditableEntity
    {
        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;
    }
}