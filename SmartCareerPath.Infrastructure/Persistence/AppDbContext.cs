using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Seeker> Seekers => Set<Seeker>();
        public DbSet<Mentor> Mentors => Set<Mentor>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<LookupType> LookupTypes => Set<LookupType>();
        public DbSet<LookupValue> LookupValues => Set<LookupValue>();
        public DbSet<CareerTrack> CareerTracks => Set<CareerTrack>();
        public DbSet<Roadmap> Roadmaps => Set<Roadmap>();
        public DbSet<RoadmapItem> RoadmapItems => Set<RoadmapItem>();
        public DbSet<SeekerRoadmapProgress> SeekerRoadmapProgress => Set<SeekerRoadmapProgress>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
        public DbSet<SeekerQuestionOption> SeekerQuestionOptions => Set<SeekerQuestionOption>();
        public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<Recommendation> Recommendations => Set<Recommendation>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<Message> Messages => Set<Message>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Global cascade restrict — catch-all fallback for any FK not explicitly
            // configured in IEntityTypeConfiguration files.
            var cascadeFKs = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                .ToList();

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;



        }
    }

}

