namespace ECommercePlatform.Domain.Entities
{
    public class AdminUserRole
    {
        public Guid AdminUserId { get; set; }

        public Guid RoleId { get; set; }

        public User AdminUser { get; set; } = null!;

        public Role Role { get; set; } = null!;
    }
}