using FastEndpoints;
using KindRaise.Api.RequestDTOs;
using KindRaise.Api.ResponseDTOs;
using KindRaise.Application.Services;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace KindRaise.Api.CampaignEndpoints
{
    public class CreateCampaignEndpoint : Endpoint<CreateCampaignRequest, CreateCampaignResponse>
    {
        private readonly ICreateCampaignService _service;

        public CreateCampaignEndpoint(ICreateCampaignService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/api/campaigns");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateCampaignRequest request, CancellationToken cancellationToken)
        {
            var applicationRequest = new Application.Campaigns.CreateCampaign.CreateCampaignRequest
            {
                Title = request.Title,
                Description = request.Description,
                MonetaryGoal = request.MonetaryGoal,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var result = await _service.ExecuteAsync(applicationRequest, cancellationToken);

            var response = new CreateCampaignResponse
            {
                Id = result.Id,
                Title = request.Title,
                Description = request.Description,
                MonetaryGoal = request.MonetaryGoal,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DonatedAmount = result.DonatedAmount,
                CampaignState = result.CampaignState
            };
            
            await Send.CreatedAtAsync<CreateCampaignEndpoint>(
                new { id = response.Id }, 
                response, 
                Http.POST, 
                cancellation: cancellationToken);
        }
    }
}
