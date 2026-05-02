using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{

    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("AspNetUsers");

            builder.Property(u => u.FirstName)
               .IsRequired()
               .HasMaxLength(50);


            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasOne(u => u.RefreshToken)
                   .WithOne(r => r.User)
                   .HasForeignKey<RefreshToken>(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);


        }
    }

}
