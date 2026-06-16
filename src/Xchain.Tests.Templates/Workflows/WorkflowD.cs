namespace Xchain.Tests.Templates;

// Exercises the generator's timeout path: Then<T>(TimeSpan) emits a helper fixture subclass
// rather than CollectionChainNextDefinition.
public class WorkflowD : WorkflowChain
{
    protected override void Configure(IWorkflowBuilder b) => b
        .Start<FlowD.Step_01>()
        .Then<FlowD.Step_02>(TimeSpan.FromMinutes(30));
}
