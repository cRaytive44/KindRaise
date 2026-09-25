using FastEndpoints;
using KindRaise.Application.Donations.GetDonations;
using KindRaise.Application.Services.Donations.GetDonations;

namespace KindRaise.Api.DonationEndpoints
{
    public class ListDonationEndpoint : Endpoint<GetDonationsRequest, GetDonationsPagedResponse>
    {
        private readonly IGetDonationsService _service;
        public ListDonationEndpoint(IGetDonationsService service)
        {
            _service = service;
        }
        public override void Configure()
        {
            Get("/api/donations");
            AllowAnonymous();
        }
        public override async Task HandleAsync(GetDonationsRequest request, CancellationToken cancellationToken)
        {
            var donations = await _service.GetAllAsync(request, cancellationToken);
            
            await Send.OkAsync(donations, cancellationToken);
        }
    }
}
