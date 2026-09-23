using KindRaise.Application.Donations.GetDonations;

namespace KindRaise.Application.Services.GetDonations
{
    public interface IGetDonationsService
    {
        Task<GetDonationsPagedResponse> GetAllAsync(
            GetDonationsRequest request, 
            CancellationToken cancellationToken);
    }
}
