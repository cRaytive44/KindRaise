using KindRaise.Application.Messaging;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class FailOnceOutboxMessagePublisher : IOutboxMessagePublisher
    {
        public int Calls { get; private set; }
        public List<Guid> MessageIds { get; } = [];

        public Task PublishAsync(
            Guid messageId,
            string type,
            string content,
            CancellationToken cancellationToken)
        {
            Calls++;
            MessageIds.Add(messageId);

            if (Calls == 1)
            {
                throw new InvalidOperationException("Simulated RabbitMQ publishing failure.");
            }

            return Task.CompletedTask;
        }
    }
}
