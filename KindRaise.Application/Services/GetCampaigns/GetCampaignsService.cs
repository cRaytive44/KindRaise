using KindRaise.Application.Campaigns.GetCampaigns;
using KindRaise.Domain.Campaign;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Application.Services.GetCampaigns
{
    public class GetCampaignsService : IGetCampaignsService
    {
        private readonly KindRaiseDbContext _dbContext;

        public GetCampaignsService(KindRaiseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetCampaignsPagedResponse> GetAllAsync(
            GetCampaignsRequest request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Campaigns
                .AsNoTracking()
                .AsQueryable();

            query = StartDateFilter(query, request);
            query = EndDateFilter(query, request);
            query = MonetaryGoalFilter(query, request);
            query = DonatedAmountFilter(query, request);
            query = CampaignStateFilter(query, request);
  
            query = query.OrderByDescending(c => c.StartDate).ThenBy(c => c.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            // Pagination
            var campaigns = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = campaigns
                .Select(c => new GetCampaignsResponse
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    MonetaryGoal = c.MonetaryGoal,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    DonatedAmount = c.DonatedAmount,
                    CampaignState = c.CampaignState
                })
                .ToList();

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / request.PageSize);

            return new GetCampaignsPagedResponse
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

        }

        private IQueryable<Campaign> StartDateFilter(
            IQueryable<Campaign> query,
            GetCampaignsRequest request)
        {

            if (request.StartDateFrom.HasValue)
            {
                var startDateFrom = request.StartDateFrom.Value.ToUniversalTime();
                query = query.Where(c => c.StartDate >= startDateFrom);
            }

            if (request.StartDateTo.HasValue)
            {
                var startDateTo = request.StartDateTo.Value.ToUniversalTime();
                query = query.Where(c => c.StartDate <= startDateTo);
            }

            return query;
        }

        private IQueryable<Campaign> EndDateFilter(
            IQueryable<Campaign> query,
            GetCampaignsRequest request)
        {
            if (request.EndDateFrom.HasValue)
            {
                var endDateFrom = request.EndDateFrom.Value.ToUniversalTime();
                query = query.Where(c => c.EndDate >= endDateFrom);
            }

            if (request.EndDateTo.HasValue)
            {
                var endDateTo = request.EndDateTo.Value.ToUniversalTime();
                query = query.Where(c => c.EndDate <= endDateTo);
            }

            return query;
        }

        private IQueryable<Campaign> MonetaryGoalFilter(
            IQueryable<Campaign> query,
            GetCampaignsRequest request)
        {
            if (request.MinGoal.HasValue)
            {
                query = query.Where(c => c.MonetaryGoal >= request.MinGoal.Value);
            }

            if (request.MaxGoal.HasValue)
            {
                query = query.Where(c => c.MonetaryGoal <= request.MaxGoal.Value);
            }

            return query;
        }

        private IQueryable<Campaign> DonatedAmountFilter(
            IQueryable<Campaign> query,
            GetCampaignsRequest request)
        {
            if (request.MinDonatedAmount.HasValue)
            {
                query = query.Where(c => c.DonatedAmount >= request.MinDonatedAmount.Value);
            }

            if (request.MaxDonatedAmount.HasValue)
            {
                query = query.Where(c => c.DonatedAmount <= request.MaxDonatedAmount.Value);
            }

            return query;
        }

        private IQueryable<Campaign> CampaignStateFilter(
            IQueryable<Campaign> query,
            GetCampaignsRequest request)
        {
            if (request.State is null) return query;

            var now = DateTimeOffset.UtcNow;

            if (request.State == CampaignState.Active)
            {
                query = query.Where(c =>
                    c.StartDate <= now &&
                    c.EndDate >= now);
            }
            else if (request.State == CampaignState.Inactive)
            {
                query = query.Where(c =>
                    c.StartDate > now ||
                    c.EndDate < now);
            }

            return query;
        }
    }
}
