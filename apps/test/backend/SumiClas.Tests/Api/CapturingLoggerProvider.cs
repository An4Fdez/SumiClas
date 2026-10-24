namespace SumiClas.Tests.Api;

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public IReadOnlyCollection<LogEntry> Entries => this._entries.ToArray();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(this._entries);

    public void Dispose()
    {
    }

    public sealed record LogEntry(LogLevel Level, Exception? Exception, string Message);

    private sealed class CapturingLogger(ConcurrentQueue<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => entries.Enqueue(new LogEntry(logLevel, exception, formatter(state, exception)));
    }
}
