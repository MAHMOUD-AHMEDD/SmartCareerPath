using SmartCareerPath.Application.DTOs.Admin;

namespace SmartCareerPath.Application.Interfaces
{
    public interface ILookupService
    {
        // LookupTypes
        Task<IEnumerable<LookupTypeDto>> GetAllTypesAsync();
        Task<LookupTypeDto> GetTypeByIdAsync(int id);
        Task<LookupTypeDto> CreateTypeAsync(CreateLookupTypeDto dto);
        Task<LookupTypeDto> UpdateTypeAsync(int id, UpdateLookupTypeDto dto);
        Task DeleteTypeAsync(int id);

        // LookupValues
        Task<IEnumerable<LookupValueDto>> GetValuesByTypeAsync(int lookupTypeId);
        Task<LookupValueDto> CreateValueAsync(CreateLookupValueDto dto);
        Task<LookupValueDto> UpdateValueAsync(int id, UpdateLookupValueDto dto);
        Task DeleteValueAsync(int id);
    }
}
