using FluentValidation;
using SmartCareerPath.Application.DTOs.Admin;

namespace SmartCareerPath.Application.Validator.Lookup
{
    public class CreateLookupValueDtoValidator : AbstractValidator<CreateLookupValueDto>
    {
        public CreateLookupValueDtoValidator()
        {
            RuleFor(x => x.LookupTypeId)
                .GreaterThan(0).WithMessage("A valid LookupTypeId is required.");
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Lookup value is required.")
                .MaximumLength(150).WithMessage("Value cannot exceed 150 characters.");
        }
    }
}
