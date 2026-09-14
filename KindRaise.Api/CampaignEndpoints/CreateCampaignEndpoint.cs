using FastEndpoints;
using KindRaise.Api.RequestDTOs;
using KindRaise.Api.ResponseDTOs;

namespace KindRaise.Api.CampaignEndpoints
{
    public class CreateCampaignEndpoint : Endpoint<CreateCampaignRequest, CreateCampaignResponse>
    {
        public override void Configure()
        {
            Post("/api/campaigns");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateCampaignRequest request, CancellationToken cancellationToken)
        {
            var response = new CreateCampaignResponse
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                MonetaryGoal = request.MonetaryGoal
            };
            
            await Send.CreatedAtAsync<CreateCampaignEndpoint>(
                new { id = response.Id }, 
                response, 
                Http.POST, 
                cancellation: cancellationToken);
        }
    }
}
