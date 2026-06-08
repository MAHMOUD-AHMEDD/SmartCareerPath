using FluentValidation;
using SmartCareerPath.Application.DTOs.RoadmapItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Validator.AdminCRUD
{
    public class CreateRoadmapItemValidator : AbstractValidator<CreateRoadmapItemDto>
    {
        public CreateRoadmapItemValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Link).Must(url =>
                url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Link must be a valid URL.");
        }
    }
    public class UpdateRoadmapItemValidator : AbstractValidator<UpdateRoadmapItemDto>
    {
        public UpdateRoadmapItemValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Link).Must(url =>
                url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Link must be a valid URL.");
        }
    }
}
