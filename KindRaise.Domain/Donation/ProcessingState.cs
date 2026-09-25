namespace KindRaise.Domain.Donation
{
    public enum ProcessingState
    {
        Pending,
        Processing,
        Processed,
        TemporaryFailure,
        Rejected
    }
}
