using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class CareerTrackConfiguration : IEntityTypeConfiguration<CareerTrack>
    {
        public void Configure(EntityTypeBuilder<CareerTrack> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(t => t.Name)
               .IsRequired()
               .HasMaxLength(100);
            builder.Property(t => t.Description)
                   .IsRequired()
                   .HasMaxLength(500);


            builder.HasIndex(t => t.Name)
              .IsUnique();


            // 1:Many — CareerTrack → Roadmaps
            builder.HasMany(t => t.Roadmaps)
                .WithOne(r => r.Track)
                .HasForeignKey(t => t.TrackId)
                .OnDelete(DeleteBehavior.Restrict);


            // 1:Many — CareerTrack → Mentors (configured on MentorConfiguration side)

            // 1:Many — CareerTrack → Recommendations
            builder.HasMany(t => t.Recommendations)
                   .WithOne(r => r.Track)
                   .HasForeignKey(r => r.TrackId)
                   .OnDelete(DeleteBehavior.Restrict);



        }
    }
}
