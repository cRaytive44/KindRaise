namespace KindRaise.Application.Payments
{
    public sealed class TemporaryPaymentFailureException(
        string message) : Exception(message);
}
