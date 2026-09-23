using FluentValidation;

namespace KindRaise.Application.Donations.CreateDonation
{
    public sealed class CreateDonationValidator : AbstractValidator<CreateDonationRequest>
    {
        public CreateDonationValidator()
        {
            RuleFor(x => x.CampaignId)
                .NotEmpty();

            RuleFor(x => x.DonorName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Amount)
                .GreaterThan(0);
        }
    }
}
