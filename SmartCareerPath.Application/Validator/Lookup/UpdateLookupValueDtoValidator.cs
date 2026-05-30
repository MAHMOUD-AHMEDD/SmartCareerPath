using FluentValidation;
using SmartCareerPath.Application.DTOs.Admin;

namespace SmartCareerPath.Application.Validator.Lookup
{
    public class UpdateLookupValueDtoValidator : AbstractValidator<UpdateLookupValueDto>
    {
        public UpdateLookupValueDtoValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Lookup value is required.")
                .MaximumLength(150).WithMessage("Value cannot exceed 150 characters.");
        }
    }
}
