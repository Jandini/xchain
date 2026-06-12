using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain.DependencyInjection.Logging;

public sealed class XchainMessageSinkLoggerProvider(IMessageSink messageSink) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) =>
        new XchainMessageSinkLogger(messageSink, categoryName);

    public void Dispose() { }
}

internal sealed class XchainMessageSinkLogger(IMessageSink messageSink, string categoryName) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var message = formatter(state, exception);
        var text = exception is not null
            ? $"[{logLevel}] {categoryName}: {message}{Environment.NewLine}{exception}"
            : $"[{logLevel}] {categoryName}: {message}";
        messageSink.OnMessage(new DiagnosticMessage(text));
    }
}
