using KindRaise.Application.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace KindRaise.Infrastructure.Database.Repositories
{
    public sealed class EfUnitOfWork(KindRaiseDbContext dbContext) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            if(_transaction is null)
            {
                throw new InvalidOperationException("No transaction has been started.");
            }

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            if(_transaction is null) return;
           
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}

