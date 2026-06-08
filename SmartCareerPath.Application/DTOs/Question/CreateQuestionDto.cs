using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Question
{
    public record CreateQuestionDto(
    string QuestionText,
    string QuestionType,               // "MCQ" or "OpenText"
    IEnumerable<string> Options);      // empty list for OpenText
}
