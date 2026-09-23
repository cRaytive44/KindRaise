using KindRaise.Application.Donations.GetDonation;

namespace KindRaise.Application.Services.GetDonation
{
    public interface IGetDonationService
    {
        Task<GetDonationResponse?> ExecuteAsync(
            GetDonationRequest request,
            CancellationToken cancellationToken);
    }
}
