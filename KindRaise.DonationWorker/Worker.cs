using KindRaise.DonationWorker;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using RabbitMQ.Client;

namespace DonationWorker
{
    public class Worker(
        ILogger<Worker> logger,
        ILogger<DonationRequestConsumer> consumerLogger,
        IConfiguration configuration) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new RabbitMQOptions
            {
                HostName = configuration["RabbitMQ:HostName"]
                ?? "localhost",

                Port = int.Parse(
                configuration["RabbitMQ:Port"] ?? "5672"),

                UserName = configuration["RabbitMQ:UserName"]
                ?? throw new InvalidOperationException(
                    "RabbitMQ username is not configured."),

                Password = configuration["RabbitMQ:Password"]
                ?? throw new InvalidOperationException(
                    "RabbitMQ password is not configured."),

                VirtualHost = configuration["RabbitMQ:VirtualHost"]
                ?? "/",

                ExchangeName = configuration["RabbitMQ:ExchangeName"]
                ?? "kindraise.donations"
            };

            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                ClientProvidedName = "KindRaise.DonationWorker"
            };

            logger.LogInformation(
                "Connecting to RabbitMQ at {HostName}:{Port}",
                factory.HostName,
                factory.Port);

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            logger.LogInformation("Successfully connected to RabbitMQ.");

            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            logger.LogInformation("Successfully created RabbitMQ channel.");

            await RabbitMQTopology.ConfigureAsync(
                channel,
                options,
                stoppingToken);
            logger.LogInformation("RabbitMQ topology configured successfully.");

            await channel.BasicQosAsync(
               prefetchSize: 0,
               prefetchCount: 1,
               global: false,
               cancellationToken: stoppingToken);
            logger.LogInformation("RabbitMQ QoS configured. PrefetchCount: {PrefetchCount}", 1);

            var consumer = new DonationRequestConsumer(consumerLogger);
            await consumer.StartAsync(channel, stoppingToken);

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("DonationWorker is stopping.");
            }
        }
    }
}
