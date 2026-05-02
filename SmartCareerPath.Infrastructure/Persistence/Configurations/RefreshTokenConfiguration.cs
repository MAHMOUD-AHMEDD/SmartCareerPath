using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Token)
                   .IsRequired()
                   .HasMaxLength(500);
            builder.Property(r => r.UserId)
                   .IsRequired();
            // Index on UserId for fast lookups during refresh/revoke
            builder.HasIndex(r => r.UserId);
        }
    }
}
