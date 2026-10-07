namespace KindRaise.Application.Payments
{
    public sealed record PaymentResult(
        PaymentResultStatus Status, 
        string? FailureReason = null);
}
