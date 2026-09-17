using FluentValidation;

namespace KindRaise.Application.Campaigns.CreateCampaign
{
    public sealed class CreateCampaignValidator : AbstractValidator<CreateCampaignRequest>
    {
        public CreateCampaignValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(5000);

            RuleFor(x => x.MonetaryGoal)
                .GreaterThan(0);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate);
        }
    }
}
