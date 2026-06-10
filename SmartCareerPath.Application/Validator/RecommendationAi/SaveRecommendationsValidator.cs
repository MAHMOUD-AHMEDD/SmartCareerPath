using FluentValidation;
using SmartCareerPath.Application.DTOs.RecommendationAi;

namespace SmartCareerPath.Application.Validator.RecommendationAi
{
    public class SaveRecommendationsValidator : AbstractValidator<SaveRecommendationsDto>
    {
        public SaveRecommendationsValidator()
        {
            RuleFor(x => x.SeekerId)
                .NotEmpty();

            RuleFor(x => x.Results)
                .NotEmpty()
                .WithMessage("Results must contain at least one recommendation.");



            RuleFor(x => x.Results)
                .Must(r => r.Select(x => x.TrackId).Distinct().Count() == r.Count())
                .WithMessage("Duplicate TrackId values are not allowed.");


            RuleFor(x => x.Results)
                .Must(r => r.Select(x => x.Rank).Distinct().Count() == r.Count())
                .WithMessage("Duplicate Rank values are not allowed.");

            RuleForEach(x => x.Results).ChildRules(r =>
            {
                r.RuleFor(x => x.TrackId).GreaterThan(0);
                r.RuleFor(x => x.Rank).GreaterThan(0);
            });
        }
    }
}
