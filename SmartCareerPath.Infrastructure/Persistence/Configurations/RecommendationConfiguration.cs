using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
    {
        public void Configure(EntityTypeBuilder<Recommendation> builder)
        {
            // Composite PK — one recommendation per seeker per track
            builder.HasKey(r => new { r.SeekerId, r.TrackId });
            // FKs to Seeker and CareerTrack are configured on their respective sides
        }
    }
}
