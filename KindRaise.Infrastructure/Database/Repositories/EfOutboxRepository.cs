using KindRaise.Application.Messaging;

namespace KindRaise.Infrastructure.Database.Repositories
{
    public sealed class EfOutboxRepository(
        KindRaiseDbContext dbContext) : IOutboxRepository
    {
        public async Task AddAsync(
            string type,
            string content,
            DateTimeOffset occurredAt,
            CancellationToken cancellationToken)
        {
            var message = new OutboxMessage(
                type,
                content,
                occurredAt);

            await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        }
    }
}
