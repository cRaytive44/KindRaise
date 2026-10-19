using KindRaise.Application.Messaging;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Infrastructure.Database.Repositories
{
    public sealed class EfProcessedMessageRepository(
        KindRaiseDbContext dbContext)
        : IProcessedMessageRepository
    {
        public async Task<bool> TryMarkAsProcessedAsync(string messageId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

            var affectedRows = await dbContext.Database
                .ExecuteSqlInterpolatedAsync(
                    $"""
                    INSERT INTO "ProcessedMessages" ("MessageId", "ProcessedAt")
                    VALUES ({messageId}, {DateTimeOffset.UtcNow})
                    ON CONFLICT ("MessageId") DO NOTHING
                    """,
                    cancellationToken);

            return affectedRows == 1;
        }
    }
}
