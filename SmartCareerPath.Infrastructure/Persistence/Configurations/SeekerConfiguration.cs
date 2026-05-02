using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class SeekerConfiguration : IEntityTypeConfiguration<Seeker>
    {
        public void Configure(EntityTypeBuilder<Seeker> builder)
        {
            builder.ToTable("Seekers");  // TPT


            builder.Property(s => s.LinkedIn)
              .HasMaxLength(200);

            // Many:1 — Seeker → LookupValue (CurrentJob)
            builder.HasOne(s => s.CurrentJob)
                   .WithMany()
                   .HasForeignKey(s => s.CurrentJobId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:Many — Seeker → Answers
            builder.HasMany(s => s.Answers)
                   .WithOne(a => a.Seeker)
                   .HasForeignKey(a => a.SeekerId)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:Many — Seeker → SeekerQuestionOptions (selected MCQ choices)
            builder.HasMany(s => s.SelectedOptions)
                   .WithOne(o => o.Seeker)
                   .HasForeignKey(o => o.SeekerId)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:Many — Seeker → Recommendations
            builder.HasMany(s => s.Recommendations)
                   .WithOne(r => r.Seeker)
                   .HasForeignKey(r => r.SeekerId)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:Many — Seeker → SeekerRoadmapProgress
            builder.HasMany(s => s.RoadmapProgress)
                   .WithOne(p => p.Seeker)
                   .HasForeignKey(p => p.SeekerId)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:Many — Seeker → Chats
            builder.HasMany(s => s.Chats)
                   .WithOne(c => c.Seeker)
                   .HasForeignKey(c => c.SeekerId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
