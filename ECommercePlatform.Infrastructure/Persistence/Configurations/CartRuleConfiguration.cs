using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class CartRuleConfiguration : IEntityTypeConfiguration<CartRule>
    {
        public void Configure(EntityTypeBuilder<CartRule> builder)
        {
            builder.ToTable("CartRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            builder.Property(x => x.Condition)
                .HasColumnName("condition")
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.Action)
                .HasColumnName("action")
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .IsRequired();

            builder.Property(x => x.StartsAt)
                .HasColumnName("startsAt");

            builder.Property(x => x.EndsAt)
                .HasColumnName("endsAt");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");
        }
    }
}
