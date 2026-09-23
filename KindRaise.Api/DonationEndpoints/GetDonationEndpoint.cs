using FastEndpoints;
using KindRaise.Application.Donations.GetDonation;
using KindRaise.Application.Services.GetDonation;

namespace KindRaise.Api.DonationEndpoints
{
    public class GetDonationEndpoint : EndpointWithoutRequest<ResponseDTOs.GetDonationResponse>
    {
        private readonly IGetDonationService _service;
        
        public GetDonationEndpoint(IGetDonationService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/api/donations/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            var id = Route<Guid>("id");

            var request = new GetDonationRequest { Id = id };
            var result = await _service.ExecuteAsync(request, cancellationToken);

            if (result is null)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            var response = new ResponseDTOs.GetDonationResponse
            {
                Id = result.Id,
                CampaignId = result.CampaignId,
                DonorName = result.DonorName,
                Amount = result.Amount,
                CreatedAt = result.CreatedAt,
                ProcessingState = result.ProcessingState
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
