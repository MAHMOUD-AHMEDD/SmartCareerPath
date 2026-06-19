using FluentValidation;
using SmartCareerPath.Application.DTOs.Auth;

namespace SmartCareerPath.Application.Validator.Auth
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{6}$").WithMessage("Code must be exactly 6 digits.");
            RuleFor(x => x.NewPassword)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Must contain a number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a special character.");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
