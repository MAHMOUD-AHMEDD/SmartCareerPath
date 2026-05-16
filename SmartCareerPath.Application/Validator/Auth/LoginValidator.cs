using FluentValidation;
using SmartCareerPath.Application.DTOs.Auth;
namespace SmartCareerPath.Application.Validator.Auth
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

}