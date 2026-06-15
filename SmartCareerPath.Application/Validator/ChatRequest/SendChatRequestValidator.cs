using FluentValidation;
using SmartCareerPath.Application.DTOs.ChatRequest;

namespace SmartCareerPath.Application.Validator.ChatRequest
{
    public class SendChatRequestValidator : AbstractValidator<SendChatRequestDto>
    {
        public SendChatRequestValidator()
        {
            RuleFor(x => x.MentorId).NotEmpty();
        }
    }
}
