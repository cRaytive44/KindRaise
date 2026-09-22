using KindRaise.Application.Donations.CreateDonation;

namespace KindRaise.Application.Services.CreateDonation
{
    public interface ICreateDonationService
    {
        Task<CreateDonationResponse> ExecuteAsync(
            CreateDonationRequest request,
            CancellationToken cancellationToken);
    }
}
