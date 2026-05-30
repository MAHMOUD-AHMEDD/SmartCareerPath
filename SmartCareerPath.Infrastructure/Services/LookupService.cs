using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.DTOs.Admin;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class LookupService : ILookupService
    {
        private readonly AppDbContext _db;

        // Fix: IMapper removed — it was injected but never used anywhere in this service.
        public LookupService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<LookupTypeDto>> GetAllTypesAsync()
            => await _db.LookupTypes
                .Select(t => new LookupTypeDto(t.Id, t.Name))
                .ToListAsync();

        public async Task<LookupTypeDto> GetTypeByIdAsync(int id)
        {
            var type = await _db.LookupTypes.FindAsync(id)
                ?? throw new KeyNotFoundException($"LookupType {id} not found.");
            return new LookupTypeDto(type.Id, type.Name);
        }

        public async Task<LookupTypeDto> CreateTypeAsync(CreateLookupTypeDto dto)
        {
            if (await _db.LookupTypes.AnyAsync(t => t.Name == dto.Name))
                throw new InvalidOperationException($"LookupType '{dto.Name}' already exists.");

            var type = new LookupType { Name = dto.Name };
            _db.LookupTypes.Add(type);
            await _db.SaveChangesAsync();
            return new LookupTypeDto(type.Id, type.Name);
        }

        public async Task<LookupTypeDto> UpdateTypeAsync(int id, UpdateLookupTypeDto dto)
        {
            var type = await _db.LookupTypes.FindAsync(id)
                ?? throw new KeyNotFoundException($"LookupType {id} not found.");
            type.Name = dto.Name;
            await _db.SaveChangesAsync();
            return new LookupTypeDto(type.Id, type.Name);
        }

        public async Task DeleteTypeAsync(int id)
        {
            var type = await _db.LookupTypes
                .Include(t => t.Values)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"LookupType {id} not found.");

            if (type.Values.Any())
                throw new InvalidOperationException(
                    "Cannot delete a LookupType that has values. Delete values first.");

            _db.LookupTypes.Remove(type);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<LookupValueDto>> GetValuesByTypeAsync(int lookupTypeId)
        {
            var typeExists = await _db.LookupTypes.AnyAsync(t => t.Id == lookupTypeId);
            if (!typeExists)
                throw new KeyNotFoundException($"LookupType {lookupTypeId} not found.");

            return await _db.LookupValues
                .Include(v => v.LookupType)
                .Where(v => v.LookupTypeId == lookupTypeId)
                .Select(v => new LookupValueDto(v.Id, v.LookupTypeId, v.LookupType.Name, v.Value))
                .ToListAsync();
        }

        public async Task<LookupValueDto> CreateValueAsync(CreateLookupValueDto dto)
        {
            var typeExists = await _db.LookupTypes.AnyAsync(t => t.Id == dto.LookupTypeId);
            if (!typeExists)
                throw new KeyNotFoundException($"LookupType {dto.LookupTypeId} not found.");

            var value = new LookupValue { LookupTypeId = dto.LookupTypeId, Value = dto.Value };
            _db.LookupValues.Add(value);
            await _db.SaveChangesAsync();

            var type = await _db.LookupTypes.FindAsync(dto.LookupTypeId);
            return new LookupValueDto(value.Id, value.LookupTypeId, type!.Name, value.Value);
        }

        public async Task<LookupValueDto> UpdateValueAsync(int id, UpdateLookupValueDto dto)
        {
            var value = await _db.LookupValues
                .Include(v => v.LookupType)
                .FirstOrDefaultAsync(v => v.Id == id)
                ?? throw new KeyNotFoundException($"LookupValue {id} not found.");

            value.Value = dto.Value;
            await _db.SaveChangesAsync();
            return new LookupValueDto(value.Id, value.LookupTypeId, value.LookupType.Name, value.Value);
        }

        public async Task DeleteValueAsync(int id)
        {
            var value = await _db.LookupValues.FindAsync(id)
                ?? throw new KeyNotFoundException($"LookupValue {id} not found.");
            _db.LookupValues.Remove(value);
            await _db.SaveChangesAsync();
        }
    }
}
