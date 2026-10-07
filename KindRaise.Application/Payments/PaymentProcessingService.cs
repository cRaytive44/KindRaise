using KindRaise.Application.Donations;
using KindRaise.Application.Messaging;
using KindRaise.Application.Services;
using KindRaise.Contracts.Payments;
using KindRaise.Domain.Donation;

namespace KindRaise.Application.Payments
{
    public class PaymentProcessingService(
        IDonationRepository donationRepository,
        IUnitOfWork unitOfWork,
        IPaymentProvider paymentProvider,
        IMessagePublisher messagePublisher) 
        : IPaymentProcessingService
    {
        public async Task ProcessAsync(
            Guid donationId,
            CancellationToken cancellationToken)
        {
            var donation = await donationRepository.FirstOrDefaultAsync(
                donation => donation.Id == donationId, 
                cancellationToken);

            if (donation is null) 
            {
                throw new InvalidOperationException($"Donation '{donationId}' was not found.");
            }

            if (donation.ProcessingState is
                ProcessingState.Processed or
                ProcessingState.Rejected)
            {
                return;
            }

            if (donation.ProcessingState == ProcessingState.Pending)
            {
                donation.StartProcessing();
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var paymentResult = await paymentProvider.ProcessPaymentAsync(
                donation.Id,
                donation.CampaignId,
                donation.Amount,
                cancellationToken);

            switch (paymentResult.Status)
            {
                case PaymentResultStatus.Success:
                    
                    donation.MarkAsProcessed();
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    var paymentConfirmed = new PaymentConfirmed(
                        donation.Id,
                        donation.CampaignId,
                        donation.Amount);

                    await messagePublisher.PublishAsync(paymentConfirmed, cancellationToken);
                    break;

                case PaymentResultStatus.Declined:

                    donation.MarkAsRejected();
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    var paymentFailed = new PaymentFailed(
                        donation.Id,
                        donation.CampaignId,
                        donation.Amount,
                        paymentResult.FailureReason ?? "Payment was declined.");

                    await messagePublisher.PublishAsync(paymentFailed, cancellationToken);
                    break;

                case PaymentResultStatus.TemporaryFailure:
                    throw new TemporaryPaymentFailureException(
                        paymentResult.FailureReason
                        ?? "Payment provider temporarily failed.");

                default:
                    throw new InvalidOperationException(
                        $"Unsupported payment result: {paymentResult.Status}.");
            }
        }
    }
}
