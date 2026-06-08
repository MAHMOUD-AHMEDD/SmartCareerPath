using FluentValidation;
using SmartCareerPath.Application.DTOs.CareerTrack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Validator.AdminCRUD
{
    public class CreateCareerTrackValidator : AbstractValidator<CreateCareerTrackDto>
    {
        public CreateCareerTrackValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        }
    }

    public class UpdateCareerTrackValidator : AbstractValidator<UpdateCareerTrackDto>
    {
        public UpdateCareerTrackValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        }
    }
}
