using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(500);

            builder.Property(n => n.Type)
                   .HasConversion<string>()
                   .HasMaxLength(50);

            // Index for the bell query: all notifications for a user ordered by unread first
            builder.HasIndex(n => new { n.UserId, n.IsRead });

            builder.HasOne(n => n.User)
                   .WithMany()
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
