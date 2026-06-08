using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Question
{
    public record QuestionDto(
    int Id, string QuestionText, string QuestionType,
    IEnumerable<QuestionOptionDto> Options);
}
