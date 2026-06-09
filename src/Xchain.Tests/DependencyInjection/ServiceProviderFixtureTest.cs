using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xchain.DependencyInjection;
using Xchain.Tests.DependencyInjection.Helpers;

namespace Xchain.Tests.DependencyInjection;

public sealed class ServiceProviderFixtureTest
{
    [Fact]
    public void Build_IsIdempotent_ReturnsSameProvider()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);

        var first = fixture.Build();
        var second = fixture.Build();

        Assert.Same(first, second);
    }

    [Fact]
    public void Services_BeforeBuild_Throws()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);

        Assert.Throws<InvalidOperationException>(() => fixture.Services);
    }

    [Fact]
    public void Services_AfterBuild_ReturnsSameProvider()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);

        var built = fixture.Build();

        Assert.Same(built, fixture.Services);
    }

    [Fact]
    public void Build_Configure_RegistersServices()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);

        fixture.Build(configure: (services, _) => services.AddSingleton<string>("hello"));

        var value = fixture.Services.GetRequiredService<string>();
        Assert.Equal("hello", value);
    }

    [Fact]
    public void Build_RegistersConfiguration_AsService()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        var config = fixture.Services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        Assert.NotNull(config);
    }

    [Fact]
    public void Dispose_StopsHostedService()
    {
        var sink = new SpyMessageSink();
        var spy = new FakeHostedService();

        var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddSingleton<IHostedService>(spy));

        Assert.True(spy.Started);
        fixture.Dispose();

        Assert.True(spy.Stopped);
        Assert.Contains(sink.Messages, m => m.Contains("stopped FakeHostedService"));
    }

    [Fact]
    public void Build_StartsHostedService()
    {
        var sink = new SpyMessageSink();
        var spy = new FakeHostedService();

        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddSingleton<IHostedService>(spy));

        Assert.True(spy.Started);
        Assert.Contains(sink.Messages, m => m.Contains("started FakeHostedService"));
    }

    private sealed class FakeHostedService : IHostedService
    {
        public bool Started { get; private set; }
        public bool Stopped { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Started = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stopped = true;
            return Task.CompletedTask;
        }
    }
}
