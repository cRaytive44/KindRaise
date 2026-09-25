using System.Linq.Expressions;

namespace KindRaise.Application.Donations
{
    public interface IDonationRepository
    {
        Task<Domain.Donation.Donation?> FirstOrDefaultAsync(
            Expression<Func<Domain.Donation.Donation, bool>> predicate,
            CancellationToken cancellationToken);

        Task AddAsync(
            Domain.Donation.Donation donation,
            CancellationToken cancellationToken);

        IQueryable<Domain.Donation.Donation> GetAll(
            CancellationToken cancellationToken);
    }
}
