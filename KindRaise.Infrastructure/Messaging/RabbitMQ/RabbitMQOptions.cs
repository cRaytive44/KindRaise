namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public sealed class RabbitMQOptions
    {
        public string HostName { get; init; } = "localhost";
        public int Port { get; init; } = 5672;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string VirtualHost { get; init; } = "/";

        public string ExchangeName { get; init; } = "kindraise.donations";
    }
}
