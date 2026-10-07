using KindRaise.Application.Payments;

namespace KindRaise.Infrastructure.Payments
{
    public sealed class FakePaymentProviderOptions
    {
        public PaymentResultStatus DefaultResult { get; set; } = PaymentResultStatus.Success;
        public IReadOnlyList<PaymentResultStatus> Results { get; set; } = [];
    }
}
