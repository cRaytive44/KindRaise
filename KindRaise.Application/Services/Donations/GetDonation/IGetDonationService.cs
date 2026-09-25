using KindRaise.Application.Donations.GetDonation;

namespace KindRaise.Application.Services.Donations.GetDonation
{
    public interface IGetDonationService
    {
        Task<GetDonationResponse?> ExecuteAsync(
            GetDonationRequest request,
            CancellationToken cancellationToken);
    }
}
