using FluentValidation;
using SmartCareerPath.Application.DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Validator.Chat
{
    public class CreateChatValidator : AbstractValidator<CreateChatDto>
    {
        public CreateChatValidator()
        {
            RuleFor(x => x.MentorId).NotEmpty();
        }
    }

}
