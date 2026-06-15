using FluentValidation;
using SmartCareerPath.Application.DTOs.Auth;

namespace SmartCareerPath.Application.Validator.Auth
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}
