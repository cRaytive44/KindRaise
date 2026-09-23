namespace KindRaise.Domain.Donation
{
    public enum ProcessingState
    {
        Pending,
        Processed,
        TemporaryFailure,
        Rejected
    }
}
