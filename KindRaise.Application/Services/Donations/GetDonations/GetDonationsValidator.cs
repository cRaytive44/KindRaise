using FluentValidation;
using KindRaise.Application.Donations.GetDonations;

namespace KindRaise.Application.Services.Donations.GetDonations
{
    public class GetDonationsValidator : AbstractValidator<GetDonationsRequest>
    {
        public GetDonationsValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100);

            RuleFor(x => x.MinAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinAmount.HasValue);

            RuleFor(x => x.MaxAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxAmount.HasValue);

            RuleFor(x => x)
                .Must(x =>
                    !x.MinAmount.HasValue ||
                    !x.MaxAmount.HasValue ||
                    x.MinAmount <= x.MaxAmount)
                .WithMessage(
                    "MinAmount must be less than or equal to MaxAmount.");
        }
    }
}
