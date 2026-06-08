using FluentValidation;
using SmartCareerPath.Application.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Validator.AdminCRUD
{
    public class CreateQuestionValidator : AbstractValidator<CreateQuestionDto>
    {
        public CreateQuestionValidator()
        {
            RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(500);
            RuleFor(x => x.QuestionType).Must(t => t == "MCQ" || t == "OpenText")
                .WithMessage("QuestionType must be 'MCQ' or 'OpenText'.");
        }
    }

    public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionDto>
    {
        public UpdateQuestionValidator()
        {
            RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(500);
            RuleFor(x => x.QuestionType).Must(t => t == "MCQ" || t == "OpenText")
                .WithMessage("QuestionType must be 'MCQ' or 'OpenText'.");
        }
    }
}
