using RabbitMQ.Client;

namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public sealed class RabbitMQConnection : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public RabbitMQConnection(RabbitMQOptions options)
        {
            _factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                ClientProvidedName = "KindRaise"
            };
        }

        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection is not null && _connection.IsOpen)
            {
                return _connection;
            }

            await _lock.WaitAsync(cancellationToken);

            try
            {
                if (_connection is not null && _connection.IsOpen)
                {
                    return _connection;
                }

                _connection = await _factory.CreateConnectionAsync(cancellationToken);
                return _connection;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

           _lock.Dispose();
        }
    }
}
