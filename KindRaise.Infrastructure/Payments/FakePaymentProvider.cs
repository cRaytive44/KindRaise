using KindRaise.Application.Payments;
using Microsoft.Extensions.Options;

namespace KindRaise.Infrastructure.Payments
{
    public sealed class FakePaymentProvider(
        IOptions<FakePaymentProviderOptions> options,
        FakePaymentProviderState state) 
        : IPaymentProvider
    {
        public Task<PaymentResult> ProcessPaymentAsync(
            Guid donationId,
            Guid campaignId,
            decimal amount,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var callNumber = state.NextCall();

            var configuredResults = options.Value.Results;

            var status = callNumber <= configuredResults.Count
                ? configuredResults[callNumber - 1]
                : options.Value.DefaultResult;

            var result = status switch
            {
                PaymentResultStatus.Success =>
                    new PaymentResult(PaymentResultStatus.Success),

                PaymentResultStatus.Declined =>
                    new PaymentResult(
                        PaymentResultStatus.Declined,
                        "Payment was declined by the fake provider."),

                PaymentResultStatus.TemporaryFailure =>
                    new PaymentResult(
                        PaymentResultStatus.TemporaryFailure,
                        "Temporary payment provider failure."),

                _ => throw new InvalidOperationException("Unsupported fake payment result.")
            };

            return Task.FromResult(result);
        }
    }
}
