using KindRaise.Application.Services;

namespace KindRaise.Infrastructure.Database.Repositories
{
    public sealed class EfUnitOfWork(KindRaiseDbContext dbContext) : IUnitOfWork
    {
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

