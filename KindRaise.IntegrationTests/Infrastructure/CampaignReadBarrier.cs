namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class CampaignReadBarrier
    {
        private readonly TaskCompletionSource _bothReadsCompleted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private int _readCount;

        public async Task WaitForInitialReadsAsync(CancellationToken cancellationToken)
        {
            var currentRead = Interlocked.Increment(ref _readCount);

            if (currentRead > 2) return;

            if (currentRead == 2)
            {
                _bothReadsCompleted.TrySetResult();
            }

            await _bothReadsCompleted.Task.WaitAsync(cancellationToken);
        }
    }
}
