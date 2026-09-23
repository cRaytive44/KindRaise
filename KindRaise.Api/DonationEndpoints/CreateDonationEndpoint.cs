using FastEndpoints;
using KindRaise.Api.RequestDTOs;
using KindRaise.Api.ResponseDTOs;
using KindRaise.Application.Services.CreateDonation;

namespace KindRaise.Api.DonationEndpoints
{
    public class CreateDonationEndpoint : Endpoint<CreateDonationRequest, CreateDonationResponse>
    {
        private readonly ICreateDonationService _service;

        public CreateDonationEndpoint(ICreateDonationService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/api/campaigns/{campaignId}/donations");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateDonationRequest request, CancellationToken cancellationToken)
        {
            var campaignId = Route<Guid>("campaignId");

            var applicationRequest = new Application.Donations.CreateDonation.CreateDonationRequest
            {
                CampaignId = campaignId,
                DonorName = request.DonorName,
                Amount = request.Amount
            };

            var result = await _service.ExecuteAsync(applicationRequest, cancellationToken);

            var response = new CreateDonationResponse
            {
                Id = result.Id,
                CampaignId = campaignId,
                DonorName = request.DonorName,
                Amount = request.Amount,
                CreatedAt = result.CreatedAt,
                ProcessingState = result.ProcessingState
            };

            await Send.CreatedAtAsync<CreateDonationEndpoint>(
                new { id = response.Id },
                response,
                Http.POST,
                cancellation: cancellationToken);
        }
    }
}
