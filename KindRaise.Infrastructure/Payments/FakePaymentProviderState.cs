namespace KindRaise.Infrastructure.Payments
{
    public sealed class FakePaymentProviderState
    {
        private int _callCount;
        public int CallCount => Volatile.Read(ref _callCount);

        public int NextCall()
        {
            return Interlocked.Increment(ref _callCount);
        }
    }
}
