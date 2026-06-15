using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class ChatRequestConfiguration : IEntityTypeConfiguration<ChatRequest>
    {
        public void Configure(EntityTypeBuilder<ChatRequest> builder)
        {
            builder.HasKey(r => r.Id);

            // HasConversion<string>() stores the enum as its name ("Pending", "Accepted", "Declined")
            // in the DB column rather than an integer. Human-readable in SSMS and safe to query.
            builder.Property(r => r.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            // Indexes for the two most common queries
            builder.HasIndex(r => new { r.MentorId, r.Status });
            builder.HasIndex(r => new { r.SeekerId, r.Status });

            builder.HasOne(r => r.Seeker)
                   .WithMany()
                   .HasForeignKey(r => r.SeekerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Mentor)
                   .WithMany()
                   .HasForeignKey(r => r.MentorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
