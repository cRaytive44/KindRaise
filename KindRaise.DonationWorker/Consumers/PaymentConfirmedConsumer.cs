using KindRaise.Application.Campaigns;
using KindRaise.Application.Messaging;
using KindRaise.Application.Services;
using KindRaise.Contracts.Payments;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace KindRaise.DonationWorker.Consumers
{
    public sealed class PaymentConfirmedConsumer(
        ILogger<PaymentConfirmedConsumer> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        public async Task StartAsync(IChannel channel, CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                await HandleMessageAsync(channel, eventArgs, cancellationToken);
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMQTopology.PaymentConfirmedQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Started consuming messages from queue '{QueueName}'",
                RabbitMQTopology.PaymentConfirmedQueue);
        }

        private async Task HandleMessageAsync(
            IChannel channel,
            BasicDeliverEventArgs eventArgs,
            CancellationToken cancellationToken)
        {
            var deliveryTag = eventArgs.DeliveryTag;
            var messageId = eventArgs.BasicProperties.MessageId;

            if (string.IsNullOrWhiteSpace(messageId))
            {
                logger.LogWarning(
                    "PaymentConfirmed has no MessageId. DeliveryTag: {DeliveryTag}",
                    deliveryTag);

                await channel.BasicNackAsync(
                    deliveryTag: deliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: cancellationToken);

                return;
            }

            try 
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var message = JsonSerializer.Deserialize<PaymentConfirmed>(json)
                    ?? throw new InvalidOperationException("Failed to deserialize PaymentConfirmed message.");

                await using var scope = serviceScopeFactory.CreateAsyncScope();

                var processedMessages = scope.ServiceProvider.GetRequiredService<IProcessedMessageRepository>();
                var campaigns = scope.ServiceProvider.GetRequiredService<ICampaignRepository>();

                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.BeginTransactionAsync(cancellationToken);

                try 
                {
                    var firstProcessing = await processedMessages.TryMarkAsProcessedAsync(messageId, cancellationToken);

                    logger.LogInformation(
                        "Idempotency check completed. MessageId: {MessageId}, FirstProcessing: {FirstProcessing}",
                        messageId,
                        firstProcessing);

                    if (!firstProcessing)
                    {
                        await unitOfWork.CommitTransactionAsync(cancellationToken);

                        logger.LogInformation("Duplicate PaymentConfirmed acknowledged. MessageId:'{MessageId}'", messageId);

                        await channel.BasicAckAsync(
                            deliveryTag: deliveryTag,
                            multiple: false,
                            cancellationToken: cancellationToken);

                        return;
                    }

                    var campaign = await campaigns.FirstOrDefaultAsync(
                        c => c.Id == message.CampaignId,
                        cancellationToken);

                    if (campaign is null)
                    {
                        throw new InvalidOperationException($"Campaign '{message.CampaignId}' not found.");
                    }

                    campaign.AddDonation(message.Amount);

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await unitOfWork.CommitTransactionAsync(cancellationToken);

                } catch {

                    await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    throw;
                }

                await channel.BasicAckAsync(
                    deliveryTag: deliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);

                logger.LogInformation(
                    "PaymentConfirmed processed. " +
                    "MessageId: {MessageId}, " +
                    "CampaignId: {CampaignId}, Amount: {Amount}",
                    messageId,
                    message.CampaignId,
                    message.Amount);

            } catch (JsonException exception) {

                logger.LogError(
                    exception,
                    "Invalid PaymentConfirmed message. MessageId: {MessageId}",
                    messageId);

                await channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken);

            } catch (Exception exception) {

                logger.LogError(
                    exception,
                    "Failed to process PaymentConfirmed. MessageId: {MessageId}",
                    messageId);

                await channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken);
            }
        }
    }
}
