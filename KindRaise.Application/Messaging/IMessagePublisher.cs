namespace KindRaise.Application.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(
            T message,
            CancellationToken cancellationToken);
    }
}
