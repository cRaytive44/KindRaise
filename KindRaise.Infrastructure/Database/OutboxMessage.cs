namespace KindRaise.Infrastructure.Database
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public DateTimeOffset OccurredAt { get; private set; }
        public DateTimeOffset? PublishedAt { get; private set; }
        public int Attempts { get; private set; }
        public string? LastError { get; private set; }

        private OutboxMessage() { }

        public OutboxMessage(string type, string content, DateTimeOffset occurredAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(type);
            ArgumentException.ThrowIfNullOrWhiteSpace(content);

            Id = Guid.NewGuid();
            Type = type;
            Content = content;
            OccurredAt = occurredAt;
            Attempts = 0;
        }

        public void MarkAsPublished(DateTimeOffset publishedAt)
        {
            PublishedAt = publishedAt;
            LastError = null;
        }

        public void RegisterFailure(string error)
        {
            Attempts++;
            LastError = error;
        }
    }
}
