namespace KindRaise.Application.Messaging
{
    public interface IProcessedMessageRepository
    {
        Task<bool> TryMarkAsProcessedAsync(string messageId, CancellationToken cancellationToken);
    }
}
