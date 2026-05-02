using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class RoadmapConfiguration : IEntityTypeConfiguration<Roadmap>
    {
        public void Configure(EntityTypeBuilder<Roadmap> builder)
        {
            builder.HasKey(r => r.Id);


            builder.Property(r => r.Title)
                   .IsRequired()
                   .HasMaxLength(200);
            builder.Property(r => r.Description)
                   .IsRequired()
                   .HasMaxLength(1000);
            // Unique constraint — one roadmap per track
            builder.HasIndex(r => r.TrackId)
                   .IsUnique();
            // 1:Many — Roadmap → RoadmapItems
            builder.HasMany(r => r.Items)
                   .WithOne(i => i.Roadmap)
                   .HasForeignKey(i => i.RoadmapId)
                   .OnDelete(DeleteBehavior.Restrict);
            // FK to CareerTrack is configured on CareerTrackConfiguration side




        }
    }
}
