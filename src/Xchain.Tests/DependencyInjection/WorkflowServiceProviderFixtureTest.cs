using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xchain.DependencyInjection;
using Xchain.Tests.DependencyInjection.Helpers;
using Xunit.Abstractions;

namespace Xchain.Tests.DependencyInjection;

public sealed class WorkflowServiceProviderFixtureTest : IDisposable
{
    public void Dispose()
    {
        // Reset static slots after each test to prevent cross-test contamination.
        WorkflowA.Teardown();
        WorkflowB.Teardown();
        WorkflowC.Teardown();
        UninitializedWorkflow.Teardown();
    }

    [Fact]
    public void Initialize_IsIdempotent_SameStaticProvider()
    {
        var sink = new SpyMessageSink();
        var first = new WorkflowA(sink);
        var second = new WorkflowA(sink);

        Assert.Same(first.Services, second.Services);
    }

    [Fact]
    public void Initialize_TwoDifferentConcreteTypes_DifferentStaticSlots()
    {
        var sink = new SpyMessageSink();
        var a = new WorkflowA(sink);
        var b = new WorkflowB(sink);

        // CRTP: each concrete type gets its own static field.
        Assert.NotSame(a.Services, b.Services);
    }

    [Fact]
    public void Teardown_DisposesProvider_AllowsRebuild()
    {
        var sink = new SpyMessageSink();
        var first = new WorkflowA(sink);
        var providerBefore = first.Services;

        WorkflowA.Teardown(sink);

        var second = new WorkflowA(sink);
        Assert.NotSame(providerBefore, second.Services);
    }

    [Fact]
    public void Teardown_StopsHostedService()
    {
        var sink = new SpyMessageSink();
        var spy = new FakeHostedService();
        var fixture = new WorkflowC(sink, spy);

        Assert.True(spy.Started);
        WorkflowC.Teardown(sink);

        Assert.True(spy.Stopped);
        Assert.Contains(sink.Messages, m => m.Contains("stopped FakeHostedService"));
    }

    [Fact]
    public void Services_BeforeInitialize_Throws()
    {
        var fixture = new UninitializedWorkflow(new SpyMessageSink());
        var ex = Assert.Throws<InvalidOperationException>(() => { _ = fixture.Services; });
        Assert.Contains("Initialize()", ex.Message);
    }

    // ---- Inner fixture types — each uses a unique type param to isolate static slots ----

    private sealed class WorkflowA : WorkflowServiceProviderFixture<WorkflowA>
    {
        public WorkflowA(IMessageSink sink) : base(sink) => Initialize();

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
            => services.AddSingleton("workflow-a");
    }

    private sealed class WorkflowB : WorkflowServiceProviderFixture<WorkflowB>
    {
        public WorkflowB(IMessageSink sink) : base(sink) => Initialize();

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
            => services.AddSingleton("workflow-b");
    }

    private sealed class WorkflowC : WorkflowServiceProviderFixture<WorkflowC>
    {
        private readonly FakeHostedService? _hosted;

        public WorkflowC(IMessageSink sink, FakeHostedService? hosted = null) : base(sink)
        {
            _hosted = hosted;
            Initialize();
        }

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            if (_hosted is not null)
                services.AddSingleton<IHostedService>(_hosted);
        }
    }

    private sealed class UninitializedWorkflow : WorkflowServiceProviderFixture<UninitializedWorkflow>
    {
        public UninitializedWorkflow(IMessageSink sink) : base(sink) { }
        protected override void ConfigureServices(IServiceCollection services, IConfiguration config) { }
    }

    private sealed class FakeHostedService : IHostedService
    {
        public bool Started { get; private set; }
        public bool Stopped { get; private set; }
        public Task StartAsync(CancellationToken ct) { Started = true; return Task.CompletedTask; }
        public Task StopAsync(CancellationToken ct) { Stopped = true; return Task.CompletedTask; }
    }
}
