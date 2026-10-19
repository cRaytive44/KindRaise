using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace KindRaise.IntegrationTests
{
    public sealed class XunitLoggerProvider(
    ITestOutputHelper output) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) =>
            new XunitLogger(output, categoryName);

        public void Dispose() { }

        private sealed class XunitLogger(
            ITestOutputHelper output,
            string categoryName) : ILogger
        {
            public bool IsEnabled(LogLevel logLevel) =>
                logLevel >= LogLevel.Information;

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                try
                {
                    output.WriteLine(
                        $"[{logLevel}] {categoryName}: {formatter(state, exception)}");

                    if (exception is not null)
                        output.WriteLine(exception.ToString());
                }
                catch (InvalidOperationException)
                {

                }
            }
        }
    }
}
