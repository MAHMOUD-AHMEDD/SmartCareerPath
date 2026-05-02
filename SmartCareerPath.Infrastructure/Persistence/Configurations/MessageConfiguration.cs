using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Content)
                   .IsRequired()
                   .HasMaxLength(4000);
            // Many:1 — Message → Seeker (optional sender)
            builder.HasOne(m => m.SenderSeeker)
                   .WithMany()
                   .HasForeignKey(m => m.SenderSeekerId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
            // Many:1 — Message → Mentor (optional sender)
            builder.HasOne(m => m.SenderMentor)
                   .WithMany()
                   .HasForeignKey(m => m.SenderMentorId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
            // Check constraint — exactly one sender must be set
            builder.ToTable(t => t.HasCheckConstraint(
            "CK_Message_Sender",
            "(SenderSeekerId IS NOT NULL AND SenderMentorId IS NULL) OR " +
            "(SenderSeekerId IS NULL AND SenderMentorId IS NOT NULL)"));
            // FK to Chat is configured on ChatConfiguration side
        }
    }
}
