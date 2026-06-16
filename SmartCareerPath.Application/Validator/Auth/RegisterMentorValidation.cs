using FluentValidation;
using SmartCareerPath.Application.DTOs.Auth;

namespace SmartCareerPath.Application.Validator.Auth
{
    public class RegisterMentorValidator : AbstractValidator<RegisterMentorDto>
    {
        public RegisterMentorValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.YearsOfExperience).GreaterThan(0); 
            RuleFor(x => x.Phone)
    .Matches(@"^\+?[0-9]{7,15}$")
    .When(x => x.Phone != null)
    .WithMessage("Phone must be a valid number (7–15 digits, optional + prefix).");
        }
    }
}
