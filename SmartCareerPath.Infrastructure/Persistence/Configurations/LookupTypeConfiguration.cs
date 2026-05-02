using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class LookupTypeConfiguration : IEntityTypeConfiguration<LookupType>
    {
        public void Configure(EntityTypeBuilder<LookupType> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            // Unique name — prevent duplicate lookup types like two "JobTitle" entries
            builder.HasIndex(t => t.Name)
                   .IsUnique();
            // 1:Many — LookupType → LookupValues
            builder.HasMany(t => t.Values)
                   .WithOne(v => v.LookupType)
                   .HasForeignKey(v => v.LookupTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
