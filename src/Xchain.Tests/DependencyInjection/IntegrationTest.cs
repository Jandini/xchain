using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xchain.DependencyInjection;
using Xunit.Abstractions;

namespace Xchain.Tests.DependencyInjection;

/// <summary>
/// Verifies that ServiceProviderFixture behaves correctly under xUnit's real fixture lifecycle:
/// IMessageSink is injected by xUnit, the provider is shared across all test methods in the class.
/// </summary>
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public sealed class IntegrationTest(
    TestChainContextFixture chain,
    ServiceProviderFixture sp) : IClassFixture<TestChainContextFixture>, IClassFixture<ServiceProviderFixture>
{
    private IServiceProvider Services { get; } = sp.Build(
        (services, config) =>
        {
            services.AddSingleton<IGreeter, Greeter>();
            services.AddSingleton(config.GetSection("TestSettings").Get<TestSettings>() ?? new TestSettings());
        });

    [ChainFact(Link = 1, Name = "Resolve singleton greeter")]
    public void Step1_ResolvesGreeter()
    {
        sp.Link(chain, (provider, o) =>
        {
            var greeter = provider.GetRequiredService<IGreeter>();
            o["greeting"] = greeter.Greet("world");
        });
    }

    [ChainFact(Link = 2, Name = "Singleton is shared across link calls")]
    public void Step2_GreeterIsSameInstance()
    {
        sp.Link(chain, (provider, o) =>
        {
            var greeter1 = provider.GetRequiredService<IGreeter>();
            var greeter2 = provider.GetRequiredService<IGreeter>();
            Assert.Same(greeter1, greeter2);
        });
    }

    [ChainFact(Link = 3, Name = "Output from step 1 visible in step 3")]
    public void Step3_OutputFromStep1_IsVisible()
    {
        sp.Link(chain, (_, o) =>
        {
            Assert.Equal("Hello, world!", o["greeting"]);
        });
    }

    [ChainFact(Link = 4, Name = "ILogger routes to IMessageSink")]
    public void Step4_LoggerRoutesToOutput()
    {
        sp.Link(chain, (provider, _) =>
        {
            var logger = provider.GetRequiredService<ILogger<IntegrationTest>>();
            logger.LogInformation("integration test log line");
        });
        // No assertion — this test verifies no exception is thrown when logging
    }

    [ChainFact(Link = 5, Name = "IConfiguration available in provider")]
    public void Step5_ConfigurationAvailable()
    {
        sp.Link(chain, (provider, _) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            Assert.NotNull(config);
        });
    }

    // ---- Helpers ----

    private interface IGreeter { string Greet(string name); }
    private sealed class Greeter : IGreeter { public string Greet(string name) => $"Hello, {name}!"; }
    private sealed class TestSettings { public string? Environment { get; set; } }
}

/// <summary>
/// Verifies the workflow scope: two separate fixture instances sharing the same static provider,
/// simulating the across-collection-boundary property (collection A's fixture disposed, collection B's
/// fixture created — both resolve to the same static provider built exactly once).
/// </summary>
public sealed class WorkflowScopeAcrossBoundaryTest : IDisposable
{
    public void Dispose() => BoundaryWorkflow.Teardown();

    [Fact]
    public void StaticProvider_SurvivesFixtureDisposal_SameInstance()
    {
        // Simulate collection A: create fixture, initialize, record provider reference, discard fixture.
        // WorkflowServiceProviderFixture is not IDisposable — xUnit won't auto-dispose it; we do it
        // manually here by simply letting the reference go out of scope (no dispose call needed).
        var sink = new Xchain.Tests.DependencyInjection.Helpers.SpyMessageSink();

        var fixtureA = new BoundaryWorkflow(sink);
        var providerFromA = fixtureA.Services;
        fixtureA = null!; // GC can collect; static _provider is unaffected

        // Simulate collection B: create a fresh fixture instance — should reuse the static provider.
        var fixtureB = new BoundaryWorkflow(sink);
        var providerFromB = fixtureB.Services;

        Assert.Same(providerFromA, providerFromB);
    }

    [Fact]
    public void StaticProvider_ServiceResolvable_BothInstances()
    {
        var sink = new Xchain.Tests.DependencyInjection.Helpers.SpyMessageSink();

        var fixtureA = new BoundaryWorkflow(sink);
        var counterA = fixtureA.Services.GetRequiredService<RequestCounter>();

        var fixtureB = new BoundaryWorkflow(sink);
        var counterB = fixtureB.Services.GetRequiredService<RequestCounter>();

        // Same singleton instance across both "collections"
        Assert.Same(counterA, counterB);
    }

    // ---- Shared workflow fixture type ----

    private sealed class BoundaryWorkflow : WorkflowServiceProviderFixture<BoundaryWorkflow>
    {
        public BoundaryWorkflow(IMessageSink sink) : base(sink) => Initialize();

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
            => services.AddSingleton<RequestCounter>();
    }

    private sealed class RequestCounter { public int Count { get; set; } }
}
