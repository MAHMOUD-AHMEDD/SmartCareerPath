using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Enums;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class SeekerRoadmapProgressConfiguration : IEntityTypeConfiguration<SeekerRoadmapProgress>
    {
        public void Configure(EntityTypeBuilder<SeekerRoadmapProgress> builder)
        {
            builder.HasKey(p => new { p.SeekerId, p.RoadmapItemId });

            builder.Property(p => p.Status)
               .IsRequired()
               .HasMaxLength(20)
               .HasDefaultValue(nameof(RoadmapItemStatus.NotStarted));
            // FKs are configured on SeekerConfiguration and RoadmapItemConfiguration sides
        }
    }
}
