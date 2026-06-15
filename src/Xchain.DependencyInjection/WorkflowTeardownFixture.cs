using Xunit;
using Xunit.Abstractions;

namespace Xchain.DependencyInjection;

public sealed class WorkflowTeardownFixture<TWorkflow>(IMessageSink sink) : IAsyncLifetime
    where TWorkflow : WorkflowServiceProviderFixture<TWorkflow>
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => WorkflowServiceProviderFixture<TWorkflow>.TeardownAsync(sink);
}
