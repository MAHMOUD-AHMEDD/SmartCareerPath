using FluentValidation;
using SmartCareerPath.Application.DTOs.SeekerProfile;

namespace SmartCareerPath.Application.Validator.SeekerProfile
{
    public class UpdateSeekerProfileValidator : AbstractValidator<UpdateSeekerProfileDto>
    {
        public UpdateSeekerProfileValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LinkedIn).Must(url =>
                url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("LinkedIn must be a valid URL.");
        }
    }
}
