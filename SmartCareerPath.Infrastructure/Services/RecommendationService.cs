using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.DTOs.Recommendation;
using SmartCareerPath.Application.DTOs.RecommendationAi;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _db;

        public RecommendationService(AppDbContext db) => _db = db;

        public async Task<SaveRecommendationsResultDto> SaveRecommendationsAsync(
            SaveRecommendationsDto dto)
        {
            var seeker = await _db.Seekers.FindAsync(dto.SeekerId)
                ?? throw new KeyNotFoundException(
                    $"Seeker '{dto.SeekerId}' not found.");

            var trackIds = dto.Results.Select(r => r.TrackId).ToList();
            var existingTrackIds = await _db.CareerTracks
                .Where(t => trackIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            var missingTracks = trackIds.Except(existingTrackIds).ToList();
            if (missingTracks.Any())
                throw new KeyNotFoundException(
                    $"Tracks not found: {string.Join(", ", missingTracks)}");

            var recommendations = dto.Results.Select(r => new Recommendation
            {
                SeekerId = dto.SeekerId,
                TrackId = r.TrackId,
                Rank = r.Rank,
                RecommendedAt = DateTime.UtcNow
            }).ToList();



            await using var tx = await _db.Database.BeginTransactionAsync();

            var existing = await _db.Recommendations
                .Where(r => r.SeekerId == dto.SeekerId)
                .ToListAsync();
            _db.Recommendations.RemoveRange(existing);

            // Flush the delete first — required to avoid EF Core change tracker conflict
            // on the composite PK (SeekerId + TrackId) when the same tracks appear in both sets.
            await _db.SaveChangesAsync();

            _db.Recommendations.AddRange(recommendations);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            return new SaveRecommendationsResultDto(
                SeekerId: dto.SeekerId,
                SavedCount: recommendations.Count,
                SavedAt: DateTime.UtcNow
            );
        }

        public async Task<IEnumerable<RecommendationDto>> GetSeekerRecommendationsAsync(
            string seekerId)
        {
            return await _db.Recommendations
                .Include(r => r.Track)
                .Where(r => r.SeekerId == seekerId)
                .OrderBy(r => r.Rank)
                .Select(r => new RecommendationDto(
                    r.TrackId, r.Track.Name, r.Track.Description,
                    r.Rank, r.RecommendedAt
                ))
                .ToListAsync();
        }
    }
}
