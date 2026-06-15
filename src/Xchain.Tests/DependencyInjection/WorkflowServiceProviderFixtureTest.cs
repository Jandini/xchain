using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xchain.DependencyInjection;
using Xchain.Tests.DependencyInjection.Helpers;
using Xunit.Abstractions;

namespace Xchain.Tests.DependencyInjection;

public sealed class WorkflowServiceProviderFixtureTest : IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Reset static slots after each test to prevent cross-test contamination.
        await WorkflowA.TeardownAsync();
        await WorkflowB.TeardownAsync();
        await WorkflowC.TeardownAsync();
        await WorkflowD.TeardownAsync();
        await WorkflowE.TeardownAsync();
        await UninitializedWorkflow.TeardownAsync();
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
    public async Task TeardownAsync_DisposesProvider_AllowsRebuild()
    {
        var sink = new SpyMessageSink();
        var first = new WorkflowA(sink);
        var providerBefore = first.Services;

        await WorkflowA.TeardownAsync(sink);

        var second = new WorkflowA(sink);
        Assert.NotSame(providerBefore, second.Services);
    }

    [Fact]
    public async Task TeardownAsync_StopsHostedService()
    {
        var sink = new SpyMessageSink();
        var spy = new FakeHostedService();
        _ = new WorkflowC(sink, spy);

        Assert.True(spy.Started);
        await WorkflowC.TeardownAsync(sink);

        Assert.True(spy.Stopped);
        Assert.Contains(sink.Messages, m => m.Contains("stopped FakeHostedService"));
    }

    [Fact]
    public async Task WorkflowTeardownFixture_DisposeAsync_StopsHostedServiceAndDisposesProvider()
    {
        var sink = new SpyMessageSink();
        var spy = new FakeHostedService();
        _ = new WorkflowD(sink, spy);

        var teardown = new WorkflowTeardownFixture<WorkflowD>(sink);
        await teardown.DisposeAsync();

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

    [Fact]
    public async Task TeardownAsync_WithAsyncOnlyDisposableService_DisposesService()
    {
        var sink = new SpyMessageSink();
        var workflow = new WorkflowE(sink);

        // Resolve so the container creates and tracks the instance for disposal.
        var asyncDisposable = workflow.Services.GetRequiredService<PublicAsyncOnlyDisposable>();

        await WorkflowE.TeardownAsync(sink);

        Assert.True(asyncDisposable.Disposed);
    }

    [Fact]
    public async Task TeardownAsync_IsIdempotent()
    {
        var sink = new SpyMessageSink();
        _ = new WorkflowA(sink);

        await WorkflowA.TeardownAsync(sink);
        var ex = await Record.ExceptionAsync(() => WorkflowA.TeardownAsync(sink));

        Assert.Null(ex);
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

    private sealed class WorkflowD : WorkflowServiceProviderFixture<WorkflowD>
    {
        private readonly FakeHostedService? _hosted;

        public WorkflowD(IMessageSink sink, FakeHostedService? hosted = null) : base(sink)
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

    private sealed class WorkflowE : WorkflowServiceProviderFixture<WorkflowE>
    {
        public WorkflowE(IMessageSink sink) : base(sink) => Initialize();

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
            => services.AddSingleton<PublicAsyncOnlyDisposable>();
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
