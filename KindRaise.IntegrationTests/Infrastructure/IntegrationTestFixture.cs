using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres =
            new PostgreSqlBuilder("postgres:17")
            .WithDatabase("KindRaiseTestDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private readonly RabbitMqContainer _rabbitMq =
        new RabbitMqBuilder("rabbitmq:4-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        public string PostgreSqlConnectionString => _postgres.GetConnectionString();
        public string RabbitMqHost => _rabbitMq.Hostname;
        public int RabbitMqPort => _rabbitMq.GetMappedPublicPort(5672);
        public string RabbitMqUsername => "guest";
        public string RabbitMqPassword => "guest";

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
            await _rabbitMq.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _rabbitMq.DisposeAsync();
            await _postgres.DisposeAsync();
        }
    }
}
