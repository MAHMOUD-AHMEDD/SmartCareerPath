using SmartCareerPath.Application.DTOs.Question;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly AppDbContext _db;
        public QuestionService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<QuestionDto>> GetAllAsync()
        {
            // Fix: EF Core cannot translate a C# static method (MapToDto) inside Select() to SQL.
            // Materialize the query first with ToListAsync(), then map in C# memory.
            var questions = await _db.Questions
                .Include(q => q.Options)
                .ToListAsync();
            return questions.Select(MapToDto);
        }

        public async Task<QuestionDto> GetByIdAsync(int id)
        {
            var q = await _db.Questions.Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id)
                ?? throw new KeyNotFoundException($"Question {id} not found.");
            return MapToDto(q);
        }

        public async Task<QuestionDto> CreateAsync(CreateQuestionDto dto)
        {
            if (dto.QuestionType == "MCQ" && !dto.Options.Any())
                throw new InvalidOperationException(
                    "MCQ questions must have at least one option.");

            if (dto.QuestionType == "OpenText" && dto.Options.Any())
                throw new InvalidOperationException(
                    "OpenText questions cannot have options.");

            var question = new Question
            {
                QuestionText = dto.QuestionText,
                QuestionType = dto.QuestionType,
                Options = dto.Options.Select(o => new QuestionOption
                {
                    OptionText = o,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };
            _db.Questions.Add(question);
            await _db.SaveChangesAsync();
            return MapToDto(question);
        }

        public async Task<QuestionDto> UpdateAsync(int id, UpdateQuestionDto dto)
        {
            var question = await _db.Questions.FindAsync(id)
                ?? throw new KeyNotFoundException($"Question {id} not found.");
            question.QuestionText = dto.QuestionText;
            question.QuestionType = dto.QuestionType;
            await _db.SaveChangesAsync();
            return MapToDto(question);
        }

        public async Task DeleteAsync(int id)
        {
            var question = await _db.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id)
                ?? throw new KeyNotFoundException($"Question {id} not found.");
            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();
        }

        public async Task AddOptionAsync(int questionId, string optionText)
        {
            var question = await _db.Questions.FindAsync(questionId)
                ?? throw new KeyNotFoundException($"Question {questionId} not found.");

            if (question.QuestionType != "MCQ")
                throw new InvalidOperationException(
                    "Cannot add options to a non-MCQ question.");

            _db.QuestionOptions.Add(new QuestionOption
            {
                QuestionId = questionId,
                OptionText = optionText,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task DeleteOptionAsync(int optionId)
        {
            var option = await _db.QuestionOptions.FindAsync(optionId)
                ?? throw new KeyNotFoundException($"QuestionOption {optionId} not found.");
            _db.QuestionOptions.Remove(option);
            await _db.SaveChangesAsync();
        }

        private static QuestionDto MapToDto(Question q) => new(
            q.Id, q.QuestionText, q.QuestionType,
            q.Options.Select(o => new QuestionOptionDto(o.Id, o.OptionText)));
    }
}
