namespace KindRaise.Application.Payments
{
    public interface IPaymentProcessingService
    {
        Task ProcessAsync(Guid donationId, CancellationToken cancellationToken);
    }
}
