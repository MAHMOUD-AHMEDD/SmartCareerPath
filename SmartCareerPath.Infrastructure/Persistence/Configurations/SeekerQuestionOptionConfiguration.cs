using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entites;

namespace SmartCareerPath.Infrastructure.Persistence.Configurations
{
    public class SeekerQuestionOptionConfiguration : IEntityTypeConfiguration<SeekerQuestionOption>
    {
        public void Configure(EntityTypeBuilder<SeekerQuestionOption> builder)
        {
            // Composite PK — single select (one answer per question per seeker)
            builder.HasKey(s => new { s.SeekerId, s.QuestionId });
            // Many:1 — SeekerQuestionOption → Seeker
            // (configured on SeekerConfiguration side via HasMany)
            // Many:1 — SeekerQuestionOption → Question
            builder.HasOne(s => s.Question)
                   .WithMany()
                   .HasForeignKey(s => s.QuestionId)
                   .OnDelete(DeleteBehavior.Restrict);
            // Many:1 — SeekerQuestionOption → QuestionOption (the chosen option)
            builder.HasOne(s => s.Option)
                   .WithMany()
                   .HasForeignKey(s => s.OptionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
