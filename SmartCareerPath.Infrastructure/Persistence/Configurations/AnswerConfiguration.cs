using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.AnswerText)
                   .IsRequired()
                   .HasMaxLength(2000);
            // Unique constraint — one answer per seeker per question (upsert pattern)
            builder.HasIndex(a => new { a.SeekerId, a.QuestionId })
                   .IsUnique();
            // FKs to Seeker and Question are configured on their respective sides
        }
    }
}
