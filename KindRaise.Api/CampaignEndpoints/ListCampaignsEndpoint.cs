using FastEndpoints;
using KindRaise.Api.ResponseDTOs;

namespace KindRaise.Api.CampaignEndpoints
{
    public class ListCampaignsEndpoint : EndpointWithoutRequest<List<GetCampaignResponse>>
    {
        public override void Configure()
        {
            Get("/api/campaigns");
            AllowAnonymous();
        }

        // I need to add access to the database to retrieve the campaigns.
        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            var campaigns = new List<GetCampaignResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Campaign 1",
                    Description = "Description 1",
                    MonetaryGoal = 1000
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Campaign 2",
                    Description = "Description 2",
                    MonetaryGoal = 2000
                }
            };

            await Send.OkAsync(campaigns, cancellationToken);
        }
    }
}
