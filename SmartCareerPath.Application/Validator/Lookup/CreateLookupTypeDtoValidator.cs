using FluentValidation;
using SmartCareerPath.Application.DTOs.Admin;

namespace SmartCareerPath.Application.Validator.Lookup
{
    public class CreateLookupTypeDtoValidator : AbstractValidator<CreateLookupTypeDto>
    {
        public CreateLookupTypeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Lookup type name is required.")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters.");
        }
    }
}
