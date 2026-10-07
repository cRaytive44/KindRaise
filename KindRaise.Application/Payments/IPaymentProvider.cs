namespace KindRaise.Application.Payments
{
    public interface IPaymentProvider
    {
        Task<PaymentResult> ProcessPaymentAsync(
            Guid donationId, 
            Guid campaignId, 
            decimal amount, 
            CancellationToken cancellationToken);
    }
}
