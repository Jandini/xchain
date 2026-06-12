using Microsoft.Extensions.DependencyInjection;
using Xchain.DependencyInjection;
using Xchain.Tests.DependencyInjection.Helpers;

namespace Xchain.Tests.DependencyInjection;

public sealed class PerLinkScopeTest
{
    [Fact]
    public void Link_ScopedService_NewInstancePerCall()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddScoped<Counter>());

        var chain = new TestChainContextFixture();

        Counter? first = null;
        Counter? second = null;

        fixture.Link(chain, (sp, _) => first = sp.GetRequiredService<Counter>());
        fixture.Link(chain, (sp, _) => second = sp.GetRequiredService<Counter>());

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void Link_SingletonService_SameInstanceAcrossCalls()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddSingleton<Counter>());

        var chain = new TestChainContextFixture();

        Counter? first = null;
        Counter? second = null;

        fixture.Link(chain, (sp, _) => first = sp.GetRequiredService<Counter>());
        fixture.Link(chain, (sp, _) => second = sp.GetRequiredService<Counter>());

        Assert.Same(first, second);
    }

    [Fact]
    public void Link_Exception_PushedToChainErrors()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        var chain = new TestChainContextFixture();
        var expected = new InvalidOperationException("boom");

        Assert.Throws<InvalidOperationException>(() =>
            fixture.Link(chain, (_, _) => throw expected));

        Assert.Single(chain.Errors);
        chain.Errors.TryPeek(out var top);
        Assert.Same(expected, top?.InnerException);
    }

    [Fact]
    public async Task LinkAsync_ScopedService_NewInstancePerCall()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build(configure: (services, _) => services.AddScoped<Counter>());

        var chain = new TestChainContextFixture();

        Counter? first = null;
        Counter? second = null;

        await fixture.LinkAsync(chain, async (sp, _, ct) =>
        {
            first = sp.GetRequiredService<Counter>();
            await Task.Yield();
        });
        await fixture.LinkAsync(chain, async (sp, _, ct) =>
        {
            second = sp.GetRequiredService<Counter>();
            await Task.Yield();
        });

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task LinkAsync_Timeout_ThrowsTimeoutException()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        var chain = new TestChainContextFixture();

        await Assert.ThrowsAsync<TimeoutException>(() =>
            fixture.LinkAsync(chain, async (_, _, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }, timeOut: TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public void LinkUnless_SkipsWhenErrorPresent()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        var chain = new TestChainContextFixture();

        // Push a specific error type into chain
        Assert.Throws<InvalidOperationException>(() =>
            fixture.Link(chain, (_, _) => throw new InvalidOperationException("prior step failed")));

        var executed = false;
        Assert.Throws<Xunit.SkipException>(() =>
            fixture.LinkUnless<InvalidOperationException>(chain, (_, _) => executed = true));

        Assert.False(executed);
    }

    [Fact]
    public void Link_Output_SharedViaChain()
    {
        var sink = new SpyMessageSink();
        using var fixture = new ServiceProviderFixture(sink);
        fixture.Build();

        var chain = new TestChainContextFixture();

        fixture.Link(chain, (_, output) => output["key"] = "value");
        string? result = null;
        fixture.Link(chain, (_, output) => result = output["key"] as string);

        Assert.Equal("value", result);
    }

    private sealed class Counter { }
}
