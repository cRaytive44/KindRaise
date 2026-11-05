using KindRaise.Application.Messaging;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.DonationWorker.OutboxMessages
{
    public sealed class OutboxMessageProcessor(
        KindRaiseDbContext dbContext,
        IOutboxMessagePublisher publisher,
        ILogger<OutboxMessageProcessor> logger)
    {
        private const int BatchSize = 20;

        public async Task<bool> ProcessPendingAsync(CancellationToken cancellationToken)
        {
            var pendingIds = await dbContext.OutboxMessages
                .AsNoTracking()
                 .Where(message => message.PublishedAt == null)
                .OrderBy(message => message.OccurredAt)
                .Select(message => message.Id)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            foreach (var id in pendingIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var message = await dbContext.OutboxMessages
                    .SingleOrDefaultAsync(
                        item => item.Id == id,
                        cancellationToken);

                if (message is null || message.PublishedAt is not null)
                    continue;

                try
                {
                    await publisher.PublishAsync(
                        message.Id,
                        message.Type,
                        message.Content,
                        cancellationToken);

                    message.MarkAsPublished(DateTimeOffset.UtcNow);

                    await dbContext.SaveChangesAsync(cancellationToken);

                    logger.LogInformation(
                        "Outbox message published. MessageId: {MessageId}",
                        message.Id);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    message.RegisterFailure(exception.Message);

                    await dbContext.SaveChangesAsync(cancellationToken);

                    logger.LogError(
                        exception,
                        "Outbox publication failed. MessageId: {MessageId}",
                        message.Id);
                }
            }

            return pendingIds.Count > 0;
        }
    }
}
