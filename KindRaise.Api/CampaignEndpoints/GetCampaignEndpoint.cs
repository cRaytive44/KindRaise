using FastEndpoints;
using KindRaise.Api.ResponseDTOs;
using KindRaise.Application.Campaigns.GetCampaign;
using KindRaise.Application.Services.GetCampaign;

namespace KindRaise.Api.CampaignEndpoints
{
    public class GetCampaignEndpoint : EndpointWithoutRequest<ResponseDTOs.GetCampaignResponse>
    {
        private readonly IGetCampaignService _service;

        public GetCampaignEndpoint(IGetCampaignService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/api/campaigns/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            var id = Route<Guid>("id");

            var request = new GetCampaignRequest { Id = id };
            var result = await _service.ExecuteAsync(request, cancellationToken);

            if (result is null)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            var response = new ResponseDTOs.GetCampaignResponse
            {
                Id = result.Id,
                Title = result.Title,
                Description = result.Description,
                MonetaryGoal = result.MonetaryGoal,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                DonatedAmount = result.DonatedAmount,
                CampaignState = result.CampaignState
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
