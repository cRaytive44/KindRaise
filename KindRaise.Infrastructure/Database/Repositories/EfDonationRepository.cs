using KindRaise.Application.Donations;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KindRaise.Infrastructure.Database.Repositories
{
    public sealed class EfDonationRepository(KindRaiseDbContext dbContext) : IDonationRepository
    {
        public async Task AddAsync(
            Domain.Donation.Donation donation,
            CancellationToken cancellationToken)
        {
            await dbContext.Donations.AddAsync(donation, cancellationToken);
        }

        public async Task<Domain.Donation.Donation?> FirstOrDefaultAsync(
            Expression<Func<Domain.Donation.Donation, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await dbContext.Donations.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public IQueryable<Domain.Donation.Donation> GetAll(
            CancellationToken cancellationToken)
        {
            return dbContext.Donations.AsNoTracking().AsQueryable();
        }
    }
}
