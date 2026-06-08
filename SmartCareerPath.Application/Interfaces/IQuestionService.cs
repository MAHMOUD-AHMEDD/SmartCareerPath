using SmartCareerPath.Application.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionDto>> GetAllAsync();
        Task<QuestionDto> GetByIdAsync(int id);
        Task<QuestionDto> CreateAsync(CreateQuestionDto dto);
        Task<QuestionDto> UpdateAsync(int id, UpdateQuestionDto dto);
        Task DeleteAsync(int id);
        Task AddOptionAsync(int questionId, string optionText);
        Task DeleteOptionAsync(int optionId);
    }
}
