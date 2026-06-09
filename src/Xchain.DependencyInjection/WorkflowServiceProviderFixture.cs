using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xchain.DependencyInjection.Logging;
using Xunit.Abstractions;

namespace Xchain.DependencyInjection;

/// <summary>
/// Abstract base for a per-workflow DI fixture shared across all collections in a chain.
/// Uses CRTP so that each concrete subclass gets its own static <see cref="IServiceProvider"/> slot —
/// no cross-workflow contamination even when multiple workflows run in the same process.
/// </summary>
/// <typeparam name="TSelf">The concrete subclass type (CRTP pattern).</typeparam>
/// <remarks>
/// The static provider is built once on first <see cref="Initialize"/> call and remains alive for
/// the lifetime of the test run. In a sequential collection chain A→B→C, xUnit disposes A's fixture
/// before creating B's — ref-counting would tear down and rebuild the provider on every step.
/// Static-with-no-auto-dispose mirrors <c>CollectionChainContextFixture</c>'s pattern.
/// Call <see cref="Teardown"/> from the last collection in the chain for graceful hosted-service shutdown.
/// </remarks>
public abstract class WorkflowServiceProviderFixture<TSelf>(IMessageSink messageSink) : IServiceProviderFixture
    where TSelf : WorkflowServiceProviderFixture<TSelf>
{
    // CLR statics are per closed generic type: WorkflowServiceProviderFixture<A>._provider
    // and WorkflowServiceProviderFixture<B>._provider are independent fields.
    private static ServiceProvider? _provider;
    private static readonly object _lock = new();

    protected abstract void ConfigureServices(IServiceCollection services, IConfiguration config);

    public IServiceProvider Services => _provider
        ?? throw new InvalidOperationException(
            $"Call Initialize() in the {typeof(TSelf).Name} constructor before accessing Services.");

    /// <summary>
    /// Builds the static provider on first call. Idempotent — safe to call in every step's constructor.
    /// </summary>
    protected void Initialize()
    {
        if (_provider is not null) return;
        lock (_lock)
        {
            if (_provider is not null) return;
            var config = ServiceProviderFixture.BuildConfiguration();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddLogging(l => l.AddXchainMessageSink(messageSink));
            ConfigureServices(services, config);
            _provider = services.BuildServiceProvider();
            XchainDiHelper.StartHostedServices(_provider, messageSink);
        }
    }

    /// <summary>
    /// Stops hosted services and disposes the static provider.
    /// Call from the last collection in the workflow chain for graceful shutdown.
    /// </summary>
    public static void Teardown(IMessageSink? sink = null)
    {
        lock (_lock)
        {
            if (_provider is null) return;
            if (sink is not null)
                XchainDiHelper.StopHostedServices(_provider, sink);
            _provider.Dispose();
            _provider = null;
        }
    }
}
