using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.DTOs.QuestionnaireSubmission;
using SmartCareerPath.Application.DTOs.Recommendation;
using SmartCareerPath.Application.DTOs.RoadmapProgress;
using SmartCareerPath.Application.DTOs.SeekerProfile;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Enums;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class SeekerService : ISeekerService
    {

        private readonly AppDbContext _db;
        private readonly IAiRecommendationService _aiService;
        public SeekerService(AppDbContext db, IAiRecommendationService aiService)
        {
            _db = db;
            _aiService = aiService;
        }
        public async Task<SeekerProfileDto> GetProfileAsync(string seekerId)
        {
            var seeker = await _db.Seekers
                .Include(s => s.CurrentJob)
                .FirstOrDefaultAsync(s => s.Id == seekerId)
                ?? throw new KeyNotFoundException("Seeker not found.");

            return new SeekerProfileDto(
                seeker.Id, seeker.FirstName, seeker.LastName,
                seeker.Email!, seeker.LinkedIn,
                seeker.CurrentJobId, seeker.CurrentJob?.Value, seeker.Phone);
        }
        public async Task<IEnumerable<RecommendationDto>> GetRecommendationsAsync(string seekerId)
        {
            // Note: this returns an empty list until Phase 7 populates the Recommendations table.
            // No blocker — implement now, data appears after Phase 7 is complete.
            return await _db.Recommendations
                .Include(r => r.Track)
                .Where(r => r.SeekerId == seekerId)
                .OrderBy(r => r.Rank)
                .Select(r => new RecommendationDto(
                    r.TrackId, r.Track.Name, r.Track.Description,
                    r.Rank, r.RecommendedAt))
                .ToListAsync();
        }
        public async Task<SeekerRoadmapDto> GetRoadmapProgressAsync(string seekerId, int trackId)
        {
            var roadmap = await _db.Roadmaps
                .Include(r => r.Items.OrderBy(i => i.OrderIndex))
                .FirstOrDefaultAsync(r => r.TrackId == trackId)
                ?? throw new KeyNotFoundException($"No roadmap found for track {trackId}.");

            // Fetch this seeker's progress for all items in this roadmap
            var itemIds = roadmap.Items.Select(i => i.Id).ToList();
            var progressMap = await _db.SeekerRoadmapProgress
                .Where(p => p.SeekerId == seekerId && itemIds.Contains(p.RoadmapItemId))
                .ToDictionaryAsync(p => p.RoadmapItemId, p => p.Status);

            return new SeekerRoadmapDto(
                RoadmapId: roadmap.Id,
                Title: roadmap.Title,
                Description: roadmap.Description,
                Items: roadmap.Items.Select(item => new SeekerRoadmapItemDto(
                    Id: item.Id,
                    Title: item.Title,
                    Description: item.Description,
                    OrderIndex: item.OrderIndex,
                    Status: progressMap.TryGetValue(item.Id, out var status)
                        ? status
                        : item.DefaultStatus,  // fall back to template default
                    Link: item.Link
                ))
            );
        }

        public async Task SubmitAnswersAsync(string seekerId, SubmitAnswersDto dto)
        {
            // Validate seeker exists
            var seekerExists = await _db.Seekers.AnyAsync(s => s.Id == seekerId);
            if (!seekerExists) throw new KeyNotFoundException("Seeker not found.");

            // --- Handle MCQ answers ---
            foreach (var mcq in dto.MCQAnswers)
            {
                // Validate question exists and is MCQ type
                var question = await _db.Questions
                    .Include(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == mcq.QuestionId)
                    ?? throw new KeyNotFoundException($"Question {mcq.QuestionId} not found.");

                if (question.QuestionType != "MCQ")
                    throw new InvalidOperationException(
                        $"Question {mcq.QuestionId} is not an MCQ question.");

                if (!question.Options.Any(o => o.Id == mcq.OptionId))
                    throw new InvalidOperationException(
                        $"Option {mcq.OptionId} does not belong to question {mcq.QuestionId}.");

                // Upsert: replace if already answered
                var existing = await _db.SeekerQuestionOptions
                    .FirstOrDefaultAsync(s =>
                        s.SeekerId == seekerId && s.QuestionId == mcq.QuestionId);

                if (existing is null)
                    _db.SeekerQuestionOptions.Add(new SeekerQuestionOption
                    {
                        SeekerId = seekerId,
                        QuestionId = mcq.QuestionId,
                        OptionId = mcq.OptionId,
                        SelectedAt = DateTime.UtcNow
                    });
                else
                {
                    existing.OptionId = mcq.OptionId;
                    existing.SelectedAt = DateTime.UtcNow;
                }
            }

            // --- Handle OpenText answers ---
            foreach (var open in dto.OpenTextAnswers)
            {
                var question = await _db.Questions
                    .FirstOrDefaultAsync(q => q.Id == open.QuestionId)
                    ?? throw new KeyNotFoundException($"Question {open.QuestionId} not found.");

                if (question.QuestionType != "OpenText")
                    throw new InvalidOperationException(
                        $"Question {open.QuestionId} is not an OpenText question.");

                var existing = await _db.Answers
                    .FirstOrDefaultAsync(a =>
                        a.SeekerId == seekerId && a.QuestionId == open.QuestionId);

                if (existing is null)
                    _db.Answers.Add(new Answer
                    {
                        SeekerId = seekerId,
                        QuestionId = open.QuestionId,
                        AnswerText = open.AnswerText,
                        AnsweredAt = DateTime.UtcNow
                    });
                else
                {
                    existing.AnswerText = open.AnswerText;
                    existing.AnsweredAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();

            await _aiService.CallAndSaveAsync(seekerId);
        }

        public async Task<SeekerProfileDto> UpdateProfileAsync(
    string seekerId, UpdateSeekerProfileDto dto)
        {
            var seeker = await _db.Seekers
                .FirstOrDefaultAsync(s => s.Id == seekerId)
                ?? throw new KeyNotFoundException("Seeker not found.");

            // Validate the lookup value exists if provided
            if (dto.CurrentJobId.HasValue)
            {
                var jobExists = await _db.LookupValues.AnyAsync(v => v.Id == dto.CurrentJobId.Value);
                if (!jobExists)
                    throw new KeyNotFoundException(
                        $"LookupValue {dto.CurrentJobId} not found.");
            }

            seeker.FirstName = dto.FirstName;
            seeker.LastName = dto.LastName;
            seeker.LinkedIn = dto.LinkedIn;
            seeker.CurrentJobId = dto.CurrentJobId;
            seeker.Phone = dto.Phone;
            await _db.SaveChangesAsync();

            // Re-query to load updated navigation property for the response
            await _db.Entry(seeker).Reference(s => s.CurrentJob).LoadAsync();

            return new SeekerProfileDto(
                seeker.Id, seeker.FirstName, seeker.LastName,
                seeker.Email!, seeker.LinkedIn,
                seeker.CurrentJobId, seeker.CurrentJob?.Value, seeker.Phone);
        }


        public async Task UpdateRoadmapItemStatusAsync(
    string seekerId, int itemId, UpdateRoadmapItemStatusDto dto)
        {
            var item = await _db.RoadmapItems.FindAsync(itemId)
                ?? throw new KeyNotFoundException($"RoadmapItem {itemId} not found.");

            // true = Completed, false = NotStarted
            var status = dto.IsCompleted
                ? RoadmapItemStatus.Completed
                : RoadmapItemStatus.NotStarted;

            var progress = await _db.SeekerRoadmapProgress
                .FirstOrDefaultAsync(p => p.SeekerId == seekerId
                                       && p.RoadmapItemId == itemId);

            if (progress is null)
            {
                _db.SeekerRoadmapProgress.Add(new SeekerRoadmapProgress
                {
                    SeekerId = seekerId,
                    RoadmapItemId = itemId,
                    Status = status.ToString(),
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                progress.Status = status.ToString();
                progress.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }
}
