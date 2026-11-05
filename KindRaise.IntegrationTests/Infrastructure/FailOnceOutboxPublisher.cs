using KindRaise.Application.Messaging;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class FailOnceOutboxPublisher : IOutboxMessagePublisher
    {
        public int CallCount { get; private set; }

        public Task PublishAsync(
            Guid messageId,
            string type,
            string content,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CallCount++;

            if (CallCount == 1)
            {
                throw new InvalidOperationException("Simulated RabbitMQ failure.");
            }

            return Task.CompletedTask;
        }
    }
}
