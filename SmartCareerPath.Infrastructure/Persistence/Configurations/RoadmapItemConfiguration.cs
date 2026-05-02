using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class RoadmapItemConfiguration : IEntityTypeConfiguration<RoadmapItem>
    {
        public void Configure(EntityTypeBuilder<RoadmapItem> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Title)
                   .IsRequired()
                   .HasMaxLength(200);
            builder.Property(i => i.Description)
                   .IsRequired()
                   .HasMaxLength(1000);
            builder.Property(i => i.DefaultStatus)
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasDefaultValue("NotStarted");
            builder.Property(i => i.Link)
                   .HasMaxLength(500);
            // 1:Many — RoadmapItem → SeekerRoadmapProgress
            builder.HasMany(i => i.SeekerProgress)
                   .WithOne(p => p.RoadmapItem)
                   .HasForeignKey(p => p.RoadmapItemId)
                   .OnDelete(DeleteBehavior.Restrict);
            // FK to Roadmap is configured on RoadmapConfiguration side
        }
    }
}
