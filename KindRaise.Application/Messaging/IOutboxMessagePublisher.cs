namespace KindRaise.Application.Messaging
{
    public interface IOutboxMessagePublisher
    {
        Task PublishAsync(
            Guid messageId,
            string type,
            string content,
            CancellationToken cancellationToken);
    }
}
