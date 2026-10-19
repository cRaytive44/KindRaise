namespace KindRaise.Infrastructure.Database
{
    public sealed class ProcessedMessage
    {
        public string MessageId { get; private set; } = string.Empty;
        public DateTimeOffset ProcessedAt { get; private set; }

        private ProcessedMessage() { }

        public ProcessedMessage(string messageId, DateTimeOffset processedAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

            MessageId = messageId;
            ProcessedAt = processedAt;
        }

    }
}
