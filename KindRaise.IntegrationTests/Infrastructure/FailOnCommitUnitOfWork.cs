using KindRaise.Application.Services;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class FailOnCommitUnitOfWork(
        IUnitOfWork inner) : IUnitOfWork
    {
        public Task BeginTransactionAsync(CancellationToken cancellationToken)
            => inner.BeginTransactionAsync(cancellationToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => inner.SaveChangesAsync(cancellationToken);

        public Task CommitTransactionAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("Simulated failure before transaction commit.");

        public Task RollbackTransactionAsync(CancellationToken cancellationToken)
            => inner.RollbackTransactionAsync(cancellationToken);
    }
}
