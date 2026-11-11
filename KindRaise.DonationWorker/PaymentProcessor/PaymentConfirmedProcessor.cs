using KindRaise.Application.Campaigns;
using KindRaise.Application.Messaging;
using KindRaise.Application.Services;
using KindRaise.Contracts.Payments;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.DonationWorker.PaymentProcessor
{
    public class PaymentConfirmedProcessor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PaymentConfirmedProcessor> logger)
    {
        private const int MaxConcurrencyRetries = 3;

        public async Task ProcessAsync(
            PaymentConfirmed message,
            string messageId,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();

                var processedMessages = scope.ServiceProvider.GetRequiredService<IProcessedMessageRepository>();
                var campaigns = scope.ServiceProvider.GetRequiredService<ICampaignRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    var firstProcessing = await processedMessages.TryMarkAsProcessedAsync(
                        messageId,
                        cancellationToken);

                    if (!firstProcessing)
                    {
                        await unitOfWork.CommitTransactionAsync(cancellationToken);

                        logger.LogInformation(
                            "Duplicate PaymentConfirmed. MessageId: {MessageId}",
                            messageId);

                        return;
                    }

                    var campaign = await campaigns.FirstOrDefaultAsync(
                        c => c.Id == message.CampaignId,
                        cancellationToken);

                    if (campaign is null)
                    {
                        throw new InvalidOperationException($"Campaign {message.CampaignId} was not found.");
                    }

                    campaign.AddDonation(message.Amount);

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await unitOfWork.CommitTransactionAsync(cancellationToken);

                    logger.LogInformation(
                        "Payment confirmed. MessageId: {MessageId}, CampaignId: {CampaignId}, Amount: {Amount}",
                        messageId,
                        message.CampaignId,
                        message.Amount);

                    return;
                }

                catch (DbUpdateConcurrencyException exception)
                {
                    await unitOfWork.RollbackTransactionAsync(CancellationToken.None);

                    logger.LogWarning(
                        exception,
                        "Concurrency conflict processing MessageId {MessageId}. Attempt {Attempt}/{MaxAttempts}",
                        messageId,
                        attempt,
                        MaxConcurrencyRetries);

                    if (attempt == MaxConcurrencyRetries)
                    {
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken);
                }

                catch
                {
                    await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    throw;
                }
            }
        }
    }
}
