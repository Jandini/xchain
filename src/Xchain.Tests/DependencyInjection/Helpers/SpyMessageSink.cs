using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain.Tests.DependencyInjection.Helpers;

internal sealed class SpyMessageSink : LongLivedMarshalByRefObject, IMessageSink
{
    public List<string> Messages { get; } = [];

    public bool OnMessage(IMessageSinkMessage message)
    {
        if (message is DiagnosticMessage dm)
            Messages.Add(dm.Message);
        return true;
    }
}
