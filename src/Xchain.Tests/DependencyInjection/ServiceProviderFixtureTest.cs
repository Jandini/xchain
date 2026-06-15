using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xchain.DependencyInjection;
using Xchain.Tests.DependencyInjection.Helpers;

namespace Xchain.Tests.DependencyInjection;

public sealed class ServiceProviderFixtureTest
{
    [Fact]
    public async Task Build_IsIdempotent_ReturnsSameProvider()
    {
        var sink = new SpyMessageSink();
        await using var fixture = new ServiceProviderFixture(sink);

        var first = fixture.Build();
        var second = fixture.Build();

        Assert.Same(first, second);
    }

    [Fact]
    public async Task Services_BeforeBuild_Throws()
    {
        var sink = new SpyMessageSink();
        await using var fixture = new ServiceProviderFixture(sink);

        Assert.Throws<InvalidOperationException>(() => fixture.Services);
    }

    [Fact]
    public async Task Services_AfterBuild_ReturnsSameProvider()
    {
        var sink = new SpyMessageSink();
        await using var fixture = new ServiceProviderFixture(sink);

        var built = fixture.Build();

        Assert.Same(built, fixture.Services);
    }

    [Fact]
    public async Task Build_Configure_RegistersServices()
    {
        var sink = new SpyMessageSink();
        await using var fixture = new ServiceProviderFixture(sink);

        fixture.Build(configure: (services, _) => services.AddSingleton<string>("hello"));

        var value = fixture.Services.GetRequiredService<string>();
        Assert.Equal("hello", value);
    }

    [Fact]
    public async Task Build_RegistersConfiguration_AsService()
    {
        var sink = new SpyMessageSink();
        await using var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        var config = fixture.Services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        Assert.NotNull(config);
    }

    [Fact]
    public async Task DisposeAsync_StopsHostedService()
    {
        var sink = new SpyMessageSink();
        var spy = new FakeHostedService();

        var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddSingleton<IHostedService>(spy));

        Assert.True(spy.Started);
        await fixture.DisposeAsync();

        Assert.True(spy.Stopped);
        Assert.Contains(sink.Messages, m => m.Contains("stopped FakeHostedService"));
    }

    [Fact]
    public async Task Build_StartsHostedService()
    {
        var sink = new SpyMessageSink();
        var spy = new FakeHostedService();

        await using var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddSingleton<IHostedService>(spy));

        Assert.True(spy.Started);
        Assert.Contains(sink.Messages, m => m.Contains("started FakeHostedService"));
    }

    [Fact]
    public async Task DisposeAsync_WithAsyncOnlyDisposableService_DisposesService()
    {
        // The container must call DisposeAsync() on IAsyncDisposable-only singleton services.
        // Use a public top-level service type so the DI engine can construct it via reflection.
        var sink = new SpyMessageSink();
        var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddSingleton<PublicAsyncOnlyDisposable>());
        var asyncDisposable = fixture.Services.GetRequiredService<PublicAsyncOnlyDisposable>();

        await fixture.DisposeAsync();

        Assert.True(asyncDisposable.Disposed);
    }

    [Fact]
    public async Task DisposeAsync_WithAsyncOnlyDisposableService_DoesNotThrow()
    {
        var sink = new SpyMessageSink();
        var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddSingleton<PublicAsyncOnlyDisposable>());
        _ = fixture.Services.GetRequiredService<PublicAsyncOnlyDisposable>();

        var ex = await Record.ExceptionAsync(() => fixture.DisposeAsync().AsTask());

        Assert.Null(ex);
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        var sink = new SpyMessageSink();
        var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        await fixture.DisposeAsync();
        var ex = await Record.ExceptionAsync(() => fixture.DisposeAsync().AsTask());

        Assert.Null(ex);
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
