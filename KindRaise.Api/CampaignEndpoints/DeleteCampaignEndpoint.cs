using FastEndpoints;
using KindRaise.Application.Campaigns.DeleteCampaign;
using KindRaise.Application.Services.Campaigns.DeleteCampaign;
using KindRaise.Domain.Exceptions;

namespace KindRaise.Api.CampaignEndpoints
{
    public class DeleteCampaignEndpoint : EndpointWithoutRequest
    {
        private readonly IDeleteCampaignService _service;

        public DeleteCampaignEndpoint(IDeleteCampaignService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Delete("/api/campaigns/{id}");
            AllowAnonymous();
        }


        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            var id = Route<Guid>("id");
            var request = new DeleteCampaignRequest { Id = id };

            try
            {
                var result = await _service.DeleteAsync(request, cancellationToken);
                if (!result)
                {
                    await Send.NotFoundAsync(cancellationToken);
                    return;
                }

                await Send.NoContentAsync(cancellationToken);
            }

            catch (CampaignDeletionNotAllowedException exception) 
            {
                AddError(exception.Message);

                await Send.ErrorsAsync(
                    StatusCodes.Status409Conflict,
                    cancellationToken);
            }
        }
    }
}
