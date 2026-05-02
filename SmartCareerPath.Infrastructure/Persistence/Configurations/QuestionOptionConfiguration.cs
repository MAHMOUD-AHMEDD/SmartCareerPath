using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
    {
        public void Configure(EntityTypeBuilder<QuestionOption> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.OptionText)
                   .IsRequired()
                   .HasMaxLength(300);
            // FK to Question is configured on QuestionConfiguration side
        }
    }
}
