using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xchain.DependencyInjection;
using Xchain.DependencyInjection.Logging;
using Xchain.Tests.DependencyInjection.Helpers;

namespace Xchain.Tests.DependencyInjection;

public sealed class LoggingBridgeTest
{
    [Fact]
    public async Task ILogger_RoutesToMessageSink()
    {
        var sink = new SpyMessageSink();
        await using var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        var logger = fixture.Services.GetRequiredService<ILogger<LoggingBridgeTest>>();
        logger.LogInformation("hello from test");

        Assert.Contains(sink.Messages, m => m.Contains("hello from test"));
    }

    [Fact]
    public void MessageSink_Write_Extension_SendsDiagnosticMessage()
    {
        var sink = new SpyMessageSink();
        sink.Write("xchain test message");

        Assert.Contains(sink.Messages, m => m == "xchain test message");
    }

    [Fact]
    public void XchainMessageSinkLoggerProvider_IsPublic_CanBeInstantiatedDirectly()
    {
        var sink = new SpyMessageSink();
        using var provider = new XchainMessageSinkLoggerProvider(sink);
        var logger = provider.CreateLogger("TestCategory");

        logger.LogWarning("direct provider test");

        Assert.Contains(sink.Messages, m => m.Contains("direct provider test") && m.Contains("TestCategory"));
    }
}
