using SmartCareerPath.Application.DTOs.RoadmapItem;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Infrastructure.Persistence;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Services
{
    public class RoadmapItemService : IRoadmapItemService
    {
        private readonly AppDbContext _db;
        public RoadmapItemService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<RoadmapItemDto>> GetByRoadmapAsync(int roadmapId)
            => await _db.RoadmapItems
                .Where(i => i.RoadmapId == roadmapId)
                .OrderBy(i => i.OrderIndex)
                .Select(i => new RoadmapItemDto(
                    i.Id, i.Title, i.Description, i.OrderIndex, i.DefaultStatus, i.Link))
                .ToListAsync();

        public async Task<RoadmapItemDto> CreateAsync(int roadmapId, CreateRoadmapItemDto dto)
        {
            var roadmapExists = await _db.Roadmaps.AnyAsync(r => r.Id == roadmapId);
            if (!roadmapExists)
                throw new KeyNotFoundException($"Roadmap {roadmapId} not found.");

            // Auto-assign order index if not provided
            var maxOrder = await _db.RoadmapItems
                .Where(i => i.RoadmapId == roadmapId)
                .MaxAsync(i => (int?)i.OrderIndex) ?? 0;

            var item = new RoadmapItem
            {
                RoadmapId = roadmapId,
                Title = dto.Title,
                Description = dto.Description,
                Link = dto.Link,
                OrderIndex = dto.OrderIndex > 0 ? dto.OrderIndex : maxOrder + 1
            };
            _db.RoadmapItems.Add(item);
            await _db.SaveChangesAsync();
            return new RoadmapItemDto(
                item.Id, item.Title, item.Description, item.OrderIndex, item.DefaultStatus, item.Link);
        }

        public async Task<RoadmapItemDto> UpdateAsync(int id, UpdateRoadmapItemDto dto)
        {
            var item = await _db.RoadmapItems.FindAsync(id)
                ?? throw new KeyNotFoundException($"RoadmapItem {id} not found.");
            item.Title = dto.Title;
            item.Description = dto.Description;
            item.OrderIndex = dto.OrderIndex;
            item.Link = dto.Link;
            await _db.SaveChangesAsync();
            return new RoadmapItemDto(
                item.Id, item.Title, item.Description, item.OrderIndex, item.DefaultStatus, item.Link);
        }

        public async Task DeleteAsync(int id)
        {
            // Fix: SeekerRoadmapProgress uses DeleteBehavior.Restrict on RoadmapItemId.
            // Deleting an item with existing progress rows throws a raw DbUpdateException.
            // Guard here for a clean 400 instead.
            var item = await _db.RoadmapItems
                .Include(i => i.SeekerProgress)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new KeyNotFoundException($"RoadmapItem {id} not found.");

            if (item.SeekerProgress.Any())
                throw new InvalidOperationException(
                    "Cannot delete a roadmap item that has seeker progress records.");

            _db.RoadmapItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task ReorderAsync(int roadmapId, IEnumerable<int> orderedItemIds)
        {
            var items = await _db.RoadmapItems
                .Where(i => i.RoadmapId == roadmapId)
                .ToListAsync();

            var idList = orderedItemIds.ToList();
            for (var i = 0; i < idList.Count; i++)
            {
                var item = items.FirstOrDefault(x => x.Id == idList[i]);
                if (item is not null) item.OrderIndex = i + 1;
            }
            await _db.SaveChangesAsync();
        }
    }
}
