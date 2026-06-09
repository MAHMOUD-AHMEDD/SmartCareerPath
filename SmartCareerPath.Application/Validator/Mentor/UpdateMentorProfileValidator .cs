using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Validator.Mentor
{
	public class UpdateMentorProfileValidator : AbstractValidator<UpdateMentorProfileDto>
	{
		public UpdateMentorProfileValidator()
		{
			RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
			RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
			RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0).LessThan(60);
			RuleFor(x => x.Description).MaximumLength(1000);
			RuleFor(x => x.Company).MaximumLength(100);
			RuleFor(x => x.LinkedIn).Must(url =>
				url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
				.WithMessage("LinkedIn must be a valid URL.");
		}
	}
}
