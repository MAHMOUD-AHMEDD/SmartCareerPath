using FluentValidation;
using SmartCareerPath.Application.DTOs.Auth;

namespace SmartCareerPath.Application.Validator.Auth
{
    public class ResendConfirmationValidator : AbstractValidator<ResendConfirmationDto>
    {
        public ResendConfirmationValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}
