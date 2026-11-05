namespace KindRaise.Application.Messaging
{
    public interface IOutboxRepository
    {
        Task AddAsync(
            string type,
            string content,
            DateTimeOffset occurredAt,
            CancellationToken cancellationToken);
    }
}
