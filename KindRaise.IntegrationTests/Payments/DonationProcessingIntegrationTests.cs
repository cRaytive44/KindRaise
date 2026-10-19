using KindRaise.Application.Payments;
using KindRaise.Contracts.Donations;
using KindRaise.Contracts.Payments;
using KindRaise.Domain.Campaign;
using KindRaise.Domain.Donation;
using KindRaise.DonationWorker.Consumers;
using KindRaise.Infrastructure.Database;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using KindRaise.Infrastructure.Payments;
using KindRaise.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text.Json;
using Xunit.Abstractions;

namespace KindRaise.IntegrationTests.Payments
{
    public sealed class DonationProcessingIntegrationTests(
        IntegrationTestFixture fixture,
        ITestOutputHelper output)
        : IClassFixture<IntegrationTestFixture>
    {

        [Fact]
        public async Task DonationRequested_WhenPaymentSucceeds_DonationBecomesProcessed()
        {
            // Arrange
            var services = IntegrationTestServiceProvider.Create(fixture);
            await using var serviceProvider = services;

            await using var scope = serviceProvider.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<KindRaiseDbContext>();
            await dbContext.Database.MigrateAsync();

            var campaign = new Campaign(
                "Integration Test Campaign",
                "Campaign created for integration testing.",
                10_000m,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(30));

            var donation = new Donation(
                campaign.Id,
                "Integration Test Donor",
                50m);

            dbContext.Campaigns.Add(campaign);
            dbContext.Donations.Add(donation);

            await dbContext.SaveChangesAsync();

            var rabbitOptions = new RabbitMQOptions
            {
                HostName = fixture.RabbitMqHost,
                Port = fixture.RabbitMqPort,
                UserName = fixture.RabbitMqUsername,
                Password = fixture.RabbitMqPassword,
                VirtualHost = "/"
            };

            var factory = new ConnectionFactory
            {
                HostName = rabbitOptions.HostName,
                Port = rabbitOptions.Port,
                UserName = rabbitOptions.UserName,
                Password = rabbitOptions.Password,
                VirtualHost = rabbitOptions.VirtualHost
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await RabbitMQTopology.ConfigureAsync(
                channel,
                rabbitOptions,
                CancellationToken.None);

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: CancellationToken.None);

            var paymentConfirmedQueue = await CreateTestEventQueueAsync(
                channel,
                RabbitMQMessageRoutingKeys.PaymentConfirmed,
                CancellationToken.None);

            using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });

            var consumerLogger = loggerFactory.CreateLogger<DonationRequestConsumer>();

            var consumer = new DonationRequestConsumer(
                consumerLogger,
                scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Act
            _ = consumer.StartAsync(channel, cancellationTokenSource.Token);

            var message = new DonationRequested(
                donation.Id,
                campaign.Id,
                donation.Amount);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            await channel.BasicPublishAsync(
                exchange: RabbitMQTopology.DonationsExchange,
                routingKey: RabbitMQMessageRoutingKeys.DonationRequested,
                mandatory: false,
                basicProperties: new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent
                },
                body: body);

            // Assert
            var processedDonation =
                await WaitForDonationStateAsync(
                    fixture.PostgreSqlConnectionString,
                    donation.Id,
                    ProcessingState.Processed,
                    cancellationTokenSource.Token);

            Assert.Equal(ProcessingState.Processed, processedDonation.ProcessingState);

            var paymentConfirmed = await WaitForEventAsync<PaymentConfirmed>(
                channel,
                paymentConfirmedQueue,
                cancellationTokenSource.Token);

            Assert.Equal(donation.Id, paymentConfirmed.DonationId);
            Assert.Equal(campaign.Id, paymentConfirmed.CampaignId);
            Assert.Equal(donation.Amount, paymentConfirmed.Amount);
        }

        [Fact]
        public async Task DonationRequested_WhenPaymentTemporarilyFails_DonationIsRetriedAndProcessed()
        {
            // Arrange
            var services = IntegrationTestServiceProvider.Create(
                fixture,
                options =>
                {
                    options.Results =
                    [
                        PaymentResultStatus.TemporaryFailure,
                        PaymentResultStatus.Success
                    ];
                });

            await using var serviceProvider = services;

            Guid donationId;
            Guid campaignId;

            await using (var scope =serviceProvider.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KindRaiseDbContext>();

                await dbContext.Database.MigrateAsync();

                var campaign = new Campaign(
                    "Temporary Failure Integration Test",
                    "Campaign created for retry testing.",
                    10_000m,
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(30));

                var donation = new Donation(
                    campaign.Id,
                    "Retry Test Donor",
                    50m);

                campaignId = campaign.Id;
                donationId = donation.Id;

                dbContext.Campaigns.Add(campaign);
                dbContext.Donations.Add(donation);

                await dbContext.SaveChangesAsync();
            }

            var rabbitOptions = new RabbitMQOptions
            {
                HostName = fixture.RabbitMqHost,
                Port = fixture.RabbitMqPort,
                UserName = fixture.RabbitMqUsername,
                Password = fixture.RabbitMqPassword,
                VirtualHost = "/"
            };

            var factory = new ConnectionFactory
            {
                HostName = rabbitOptions.HostName,
                Port = rabbitOptions.Port,
                UserName = rabbitOptions.UserName,
                Password = rabbitOptions.Password,
                VirtualHost = rabbitOptions.VirtualHost
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await RabbitMQTopology.ConfigureAsync(
                channel,
                rabbitOptions,
                CancellationToken.None);

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: CancellationToken.None);

            using var loggerFactory =
                LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });

            var consumerLogger = loggerFactory.CreateLogger<DonationRequestConsumer>();
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var consumer = new DonationRequestConsumer(
                consumerLogger,
                scopeFactory);

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            
            var paymentProviderState =serviceProvider.GetRequiredService<FakePaymentProviderState>();

            // Act
            await consumer.StartAsync(channel, cancellationTokenSource.Token);

            var message = new DonationRequested(
                donationId,
                campaignId,
                50m);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            await channel.BasicPublishAsync(
                exchange: RabbitMQTopology.DonationsExchange,
                routingKey: RabbitMQMessageRoutingKeys.DonationRequested,
                mandatory: false,
                basicProperties: new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent
                },
                body: body);

            // Assert
            var processedDonation =
                await WaitForDonationStateAsync(
                    fixture.PostgreSqlConnectionString,
                    donationId,
                    ProcessingState.Processed,
                    cancellationTokenSource.Token);

            Assert.Equal(ProcessingState.Processed, processedDonation.ProcessingState);
            Assert.Equal(2, paymentProviderState.CallCount);
        }

        [Fact]
        public async Task DonationRequested_WhenSameMessageIsPublishedTwice_PaymentIsProcessedTwice()
        {
            // Arrange
            var services = IntegrationTestServiceProvider.Create(fixture);

            await using var serviceProvider = services;
            await using var scope = serviceProvider.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<KindRaiseDbContext>();

            await dbContext.Database.MigrateAsync();

            var campaign = new Campaign(
                "Idempotency Integration Test Campaign",
                "Campaign created for idempotency testing.",
                10_000m,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(30));

            var donation = new Donation(
                campaign.Id,
                "Idempotency Test Donor",
                50m);

            dbContext.Campaigns.Add(campaign);
            dbContext.Donations.Add(donation);

            await dbContext.SaveChangesAsync();

            var rabbitOptions = new RabbitMQOptions
            {
                HostName = fixture.RabbitMqHost,
                Port = fixture.RabbitMqPort,
                UserName = fixture.RabbitMqUsername,
                Password = fixture.RabbitMqPassword,
                VirtualHost = "/"
            };

            var factory = new ConnectionFactory
            {
                HostName = rabbitOptions.HostName,
                Port = rabbitOptions.Port,
                UserName = rabbitOptions.UserName,
                Password = rabbitOptions.Password,
                VirtualHost = rabbitOptions.VirtualHost
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await RabbitMQTopology.ConfigureAsync(
                channel,
                rabbitOptions,
                CancellationToken.None);

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: CancellationToken.None);

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var consumerLogger = loggerFactory.CreateLogger<DonationRequestConsumer>();

            var consumer = new DonationRequestConsumer(
                consumerLogger,
                scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            var paymentProviderState = serviceProvider.GetRequiredService<FakePaymentProviderState>();

            // Act
            _ = consumer.StartAsync(channel, cancellationTokenSource.Token);

            var message = new DonationRequested(
                donation.Id,
                campaign.Id,
                donation.Amount);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            // Publish the exact same message twice.
            await channel.BasicPublishAsync(
                exchange: RabbitMQTopology.DonationsExchange,
                routingKey: RabbitMQMessageRoutingKeys.DonationRequested,
                mandatory: false,
                basicProperties: new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent
                },
                body: body);

            await channel.BasicPublishAsync(
                exchange: RabbitMQTopology.DonationsExchange,
                routingKey: RabbitMQMessageRoutingKeys.DonationRequested,
                mandatory: false,
                basicProperties: new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent
                },
                body: body);

            // Assert
            var processedDonation =
                await WaitForDonationStateAsync(
                    fixture.PostgreSqlConnectionString,
                    donation.Id,
                    ProcessingState.Processed,
                    cancellationTokenSource.Token);

            Assert.Equal(
                ProcessingState.Processed,
                processedDonation.ProcessingState);

            Assert.Equal(1, paymentProviderState.CallCount);
        }


        [Fact]
        public async Task PaymentConfirmed_WhenSameMessageIsDeliveredTwice_CampaignIsUpdatedOnlyOnce()
        {
            // Arrange
            await using var serviceProvider = IntegrationTestServiceProvider.Create(fixture);
            Guid campaignId;

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KindRaiseDbContext>();

                await dbContext.Database.MigrateAsync();

                var campaign = new Campaign(
                    "Idempotency Integration Test",
                    "Campaign for duplicate-message testing.",
                    10_000m,
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(30));

                campaignId = campaign.Id;

                dbContext.Campaigns.Add(campaign);
                await dbContext.SaveChangesAsync();
            }

            var factory = new ConnectionFactory
            {
                HostName = fixture.RabbitMqHost,
                Port = fixture.RabbitMqPort,
                UserName = fixture.RabbitMqUsername,
                Password = fixture.RabbitMqPassword,
                VirtualHost = "/"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var consumerChannel = await connection.CreateChannelAsync();
            await using var publisherChannel = await connection.CreateChannelAsync();

            var options = new RabbitMQOptions
            {
                HostName = fixture.RabbitMqHost,
                Port = fixture.RabbitMqPort,
                UserName = fixture.RabbitMqUsername,
                Password = fixture.RabbitMqPassword,
                VirtualHost = "/"
            };

            await RabbitMQTopology.ConfigureAsync(
                consumerChannel,
                options,
                CancellationToken.None);

            await RabbitMQTopology.ConfigureAsync(
                publisherChannel,
                options,
                CancellationToken.None);

            await consumerChannel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: CancellationToken.None);

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddProvider(new XunitLoggerProvider(output));
            });

            var consumer = new PaymentConfirmedConsumer(
                loggerFactory.CreateLogger<PaymentConfirmedConsumer>(),
                serviceProvider.GetRequiredService<IServiceScopeFactory>());

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));

            await consumer.StartAsync(consumerChannel, cts.Token);

            var message = new PaymentConfirmed(
                Guid.NewGuid(),
                campaignId,
                20m);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var messageId = Guid.NewGuid().ToString();

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = messageId
            };

            // Act
            await publisherChannel.BasicPublishAsync(
                exchange: RabbitMQTopology.EventsExchange,
                routingKey: RabbitMQMessageRoutingKeys.PaymentConfirmed,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cts.Token);

            await publisherChannel.BasicPublishAsync(
                exchange: RabbitMQTopology.EventsExchange,
                routingKey: RabbitMQMessageRoutingKeys.PaymentConfirmed,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cts.Token);

            decimal lastObservedAmount = -1m;

            // Assert
            try
            {
                while (true)
                {
                    await using var dbContext = new KindRaiseDbContext(
                        new DbContextOptionsBuilder<KindRaiseDbContext>()
                            .UseNpgsql(fixture.PostgreSqlConnectionString)
                            .Options);

                    var campaign = await dbContext.Campaigns
                        .AsNoTracking()
                        .SingleAsync(c => c.Id == campaignId, cts.Token);

                    lastObservedAmount = campaign.DonatedAmount;

                    if (lastObservedAmount == 20m)
                    {
                        break;
                    }

                    await Task.Delay(100, cts.Token);
                }
            }
            catch (OperationCanceledException)
                when (cts.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Campaign '{campaignId}' did not reach 20 €. " +
                    $"Last observed amount: {lastObservedAmount} €.");
            }

            await Task.Delay(500, cts.Token);

            await using (var assertContext = new KindRaiseDbContext(
                new DbContextOptionsBuilder<KindRaiseDbContext>()
                    .UseNpgsql(fixture.PostgreSqlConnectionString)
                    .Options))
            {
                var campaign = await assertContext.Campaigns
                    .AsNoTracking()
                    .SingleAsync(c => c.Id == campaignId, cts.Token);

                var processedMessageCount =
                    await assertContext.ProcessedMessages
                        .CountAsync(m => m.MessageId == messageId, cts.Token);

                Assert.Equal(20m, campaign.DonatedAmount);
                Assert.Equal(1, processedMessageCount);
            }
        }

        private static async Task<Donation> WaitForDonationStateAsync(
             string connectionString,
             Guid donationId,
             ProcessingState expectedState,
             CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    await using var dbContext = new KindRaiseDbContext(
                        new DbContextOptionsBuilder<KindRaiseDbContext>()
                            .UseNpgsql(connectionString)
                            .Options);

                    var donation = await dbContext.Donations
                        .AsNoTracking()
                        .SingleAsync(
                            d => d.Id == donationId,
                            cancellationToken);

                    if (donation.ProcessingState == expectedState)
                    {
                        return donation;
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Donation '{donationId}' did not reach state " +
                    $"'{expectedState}' within the expected timeout.");
            }
        }

        private static async Task<string> CreateTestEventQueueAsync(
            IChannel channel,
            string routingKey,
            CancellationToken cancellationToken)
        {
            var queue = await channel.QueueDeclareAsync(
                queue: string.Empty,
                durable: false,
                exclusive: true,
                autoDelete: true,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: queue.QueueName,
                exchange: RabbitMQTopology.EventsExchange,
                routingKey: routingKey,
                cancellationToken: cancellationToken);

            return queue.QueueName;
        }

        private static async Task<T> WaitForEventAsync<T>(
            IChannel channel,
            string queueName,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                var result = await channel.BasicGetAsync(
                    queue: queueName,
                    autoAck: true,
                    cancellationToken: cancellationToken);

                if (result is not null)
                {
                    return JsonSerializer.Deserialize<T>(result.Body.Span)
                        ?? throw new InvalidOperationException(
                            $"Could not deserialize event '{typeof(T).Name}'.");
                }

                await Task.Delay(
                    TimeSpan.FromMilliseconds(100),
                    cancellationToken);
            }
        }
    }
}
