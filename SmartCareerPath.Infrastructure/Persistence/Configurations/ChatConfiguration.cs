using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.HasKey(c => c.Id);
            // Unique constraint — prevent duplicate chats between same seeker and mentor
            builder.HasIndex(c => new { c.SeekerId, c.MentorId })
                   .IsUnique();
            // 1:Many — Chat → Messages
            builder.HasMany(c => c.Messages)
                   .WithOne(m => m.Chat)
                   .HasForeignKey(m => m.ChatId)
                   .OnDelete(DeleteBehavior.Restrict);
            // FKs to Seeker and Mentor are configured on their respective sides
        }
    }
}
