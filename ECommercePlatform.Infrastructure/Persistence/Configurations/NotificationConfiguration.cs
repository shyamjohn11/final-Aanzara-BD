using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)");

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Message)
                .HasColumnName("message")
                .HasColumnType("text");

            builder.Property(x => x.IsRead)
                .HasColumnName("isRead")
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("system");

            builder.Property(x => x.Link)
                .HasColumnName("link")
                .HasMaxLength(1000);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");
        }
    }
}
