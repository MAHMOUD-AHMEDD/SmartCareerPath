using FluentValidation;
using SmartCareerPath.Application.DTOs.Roadmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Validator.AdminCRUD
{
    public class CreateRoadmapValidator : AbstractValidator<CreateRoadmapDto>
    {
        public CreateRoadmapValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        }
    }

    public class UpdateRoadmapValidator : AbstractValidator<UpdateRoadmapDto>
    {
        public UpdateRoadmapValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        }
    }
}
