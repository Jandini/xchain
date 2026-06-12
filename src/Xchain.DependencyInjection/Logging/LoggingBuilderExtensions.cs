using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Xchain.DependencyInjection.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddXchainMessageSink(this ILoggingBuilder builder, IMessageSink messageSink)
    {
        builder.AddProvider(new XchainMessageSinkLoggerProvider(messageSink));
        return builder;
    }
}
