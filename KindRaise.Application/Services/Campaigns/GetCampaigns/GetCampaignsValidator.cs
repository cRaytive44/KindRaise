using KindRaise.Application.Campaigns.GetCampaigns;
using FluentValidation;

namespace KindRaise.Application.Services.Campaigns.GetCampaigns
{
    public class GetCampaignsValidator : AbstractValidator<GetCampaignsRequest>
    {
        public GetCampaignsValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100);

            RuleFor(x => x.MinGoal)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinGoal.HasValue);

            RuleFor(x => x.MaxGoal)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxGoal.HasValue);

            RuleFor(x => x.MinDonatedAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinDonatedAmount.HasValue);

            RuleFor(x => x.MaxDonatedAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxDonatedAmount.HasValue);

            RuleFor(x => x)
                .Must(x =>
                    !x.MinGoal.HasValue ||
                    !x.MaxGoal.HasValue ||
                    x.MinGoal <= x.MaxGoal)
                .WithMessage(
                    "MinGoal must be less than or equal to MaxGoal.");

            RuleFor(x => x)
                .Must(x =>
                    !x.MinDonatedAmount.HasValue ||
                    !x.MaxDonatedAmount.HasValue ||
                    x.MinDonatedAmount <= x.MaxDonatedAmount)
                .WithMessage(
                    "MinDonatedAmount must be less than or equal to MaxDonatedAmount.");

            RuleFor(x => x)
                .Must(x =>
                    !x.StartDateFrom.HasValue ||
                    !x.StartDateTo.HasValue ||
                    x.StartDateFrom <= x.StartDateTo)
                .WithMessage(
                    "StartDateFrom must be less than or equal to StartDateTo.");

            RuleFor(x => x)
                .Must(x =>
                    !x.EndDateFrom.HasValue ||
                    !x.EndDateTo.HasValue ||
                    x.EndDateFrom <= x.EndDateTo)
                .WithMessage(
                    "EndDateFrom must be less than or equal to EndDateTo.");
        }
    }
}
