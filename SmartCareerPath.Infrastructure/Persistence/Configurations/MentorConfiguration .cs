using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class MentorConfiguration : IEntityTypeConfiguration<Mentor>
    {
        public void Configure(EntityTypeBuilder<Mentor> builder)
        {
            builder.ToTable("Mentors");  // TPT


            builder.Property(m => m.Description)
               .HasMaxLength(1000);
            builder.Property(m => m.Company)
                   .HasMaxLength(100);
            builder.Property(m => m.LinkedIn)
                   .HasMaxLength(200);
            // Many:1 — Mentor → LookupValue (CurrentJob)
            builder.HasOne(m => m.CurrentJob)
                   .WithMany()
                   .HasForeignKey(m => m.CurrentJobId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
            // Many:1 — Mentor → CareerTrack (one mentor, one track)
            builder.HasOne(m => m.Track)
                   .WithMany(t => t.Mentors)
                   .HasForeignKey(m => m.TrackId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:Many — Mentor → Chats
            builder.HasMany(m => m.Chats)
                   .WithOne(c => c.Mentor)
                   .HasForeignKey(c => c.MentorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
