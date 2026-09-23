using KindRaise.Application.Donations.GetDonations;
using KindRaise.Domain.Donation;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Application.Services.GetDonations
{
    public sealed class GetDonationsService : IGetDonationsService
    {
        private readonly KindRaiseDbContext _dbContext;

        public GetDonationsService(KindRaiseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetDonationsPagedResponse> GetAllAsync(
            GetDonationsRequest request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Donations
                .AsNoTracking()
                .AsQueryable();

            query = MonetaryGoalFilter(query, request);
            query = DonatedAmountFilter(query, request);
            query = DateFilter(query, request);
            query = CampaignStateFilter(query, request);

            query = query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            // Pagination
            var donations = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = donations
                .Select(c => new GetDonationsResponse
                {
                    Id = c.Id,
                    CampaignId = c.CampaignId,
                    DonorName = c.DonorName,
                    Amount = c.Amount,
                    CreatedAt = c.CreatedAt,
                    ProcessingState = c.ProcessingState
                })
                .ToList();

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / request.PageSize);

            return new GetDonationsPagedResponse
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        private static IQueryable<Donation> MonetaryGoalFilter(
            IQueryable<Donation> query,
            GetDonationsRequest request)
        {
            if (request.CampaignId.HasValue)
            {
                query = query.Where(c => c.CampaignId == request.CampaignId.Value);
            }
            return query;
        }

        private static IQueryable<Donation> DonatedAmountFilter(
            IQueryable<Donation> query,
            GetDonationsRequest request)
        {
            if (request.MinAmount.HasValue)
            {
                query = query.Where(c => c.Amount >= request.MinAmount.Value);
            }

            if (request.MaxAmount.HasValue)
            {
                query = query.Where(c => c.Amount <= request.MaxAmount.Value);
            }

            return query;
        }

        private static IQueryable<Donation> DateFilter(
            IQueryable<Donation> query,
            GetDonationsRequest request)
        {

            if (request.FromDate.HasValue)
            {
                var fromDate = request.FromDate.Value.ToUniversalTime();
                query = query.Where(c => c.CreatedAt >= fromDate);
            }

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value.ToUniversalTime();
                query = query.Where(c => c.CreatedAt <= toDate);
            }

            return query;
        }

        private static IQueryable<Donation> CampaignStateFilter(
            IQueryable<Donation> query,
            GetDonationsRequest request)
        {
            if (request.ProcessingState is null) return query;

            query = query.Where(c => c.ProcessingState == request.ProcessingState);

            return query;
        }

    }
}
