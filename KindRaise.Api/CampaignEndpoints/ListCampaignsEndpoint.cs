using FastEndpoints;
using KindRaise.Application.Campaigns.GetCampaigns;
using KindRaise.Application.Services.Campaigns.GetCampaigns;

namespace KindRaise.Api.CampaignEndpoints
{
    public class ListCampaignsEndpoint : Endpoint<GetCampaignsRequest, GetCampaignsPagedResponse>
    {
        private readonly IGetCampaignsService _service;

        public ListCampaignsEndpoint(IGetCampaignsService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/api/campaigns");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetCampaignsRequest request, CancellationToken cancellationToken)
        {
            var campaigns = await _service.GetAllAsync(request, cancellationToken);

            await Send.OkAsync(campaigns, cancellationToken);
        }
    }
}
