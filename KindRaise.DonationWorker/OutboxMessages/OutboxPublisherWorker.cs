using KindRaise.DonationWorker.OutboxMessages;
using KindRaise.Infrastructure.Database;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.DonationWorker.Publishers
{
    public sealed class OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        OutboxRabbitMQPublisher publisher,
        ILogger<OutboxPublisherWorker> logger)
        : BackgroundService
    {
        private const int BatchSize = 20;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Outbox publisher worker started.");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var processedAny = await ProcessBatchAsync(cancellationToken);

                    if (!processedAny)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Outbox polling failed. Retrying shortly.");

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                    catch (OperationCanceledException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }

            logger.LogInformation("Outbox publisher worker stopped.");
        }

        private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var processor = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessor>();

            return await processor.ProcessPendingAsync(cancellationToken);

        }
    }
}
