using Xunit.Abstractions;

namespace Xchain.DependencyInjection;

public sealed class WorkflowTeardownFixture<TWorkflow>(IMessageSink sink) : IDisposable
    where TWorkflow : WorkflowServiceProviderFixture<TWorkflow>
{
    public void Dispose() => WorkflowServiceProviderFixture<TWorkflow>.Teardown(sink);
}
