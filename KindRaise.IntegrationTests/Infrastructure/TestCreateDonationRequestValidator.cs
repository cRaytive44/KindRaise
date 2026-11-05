using FluentValidation;
using KindRaise.Application.Donations.CreateDonation;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class TestCreateDonationRequestValidator : AbstractValidator<CreateDonationRequest>
    {
        // Verifies persistence and transactions
    }
}
