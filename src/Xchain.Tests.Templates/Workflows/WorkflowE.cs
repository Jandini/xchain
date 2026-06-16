using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xchain.DependencyInjection;
using Xunit.Abstractions;

namespace Xchain.Tests.Templates;

// Exercises WithWorkflowFixture<T>: the generator adds ICollectionFixture<FlowEFixture>
// to every step definition and ICollectionFixture<WorkflowTeardownFixture<FlowEFixture>>
// to the last step definition.
public class WorkflowE : WorkflowChain
{
    protected override void Configure(IWorkflowBuilder b) => b
        .WithWorkflowFixture<FlowEFixture>()
        .Start<FlowE.Step_01>()
        .End<FlowE.Step_02>();
}

public class FlowEFixture : WorkflowServiceProviderFixture<FlowEFixture>
{
    public FlowEFixture(IMessageSink sink) : base(sink) => Initialize();

    protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => services.AddSingleton("flow-e");
}
