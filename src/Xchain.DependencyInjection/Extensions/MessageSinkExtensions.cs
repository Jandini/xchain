using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain.DependencyInjection;

public static class MessageSinkExtensions
{
    public static void Write(this IMessageSink sink, string message)
        => sink.OnMessage(new DiagnosticMessage(message));
}
