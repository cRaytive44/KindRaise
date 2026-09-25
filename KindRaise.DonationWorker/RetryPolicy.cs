namespace KindRaise.DonationWorker
{
    public static class RetryPolicy
    {
        public const int MaxAttempts = 3;
        public const string RetryCountHeader = "x-retry-count";
    }
}
