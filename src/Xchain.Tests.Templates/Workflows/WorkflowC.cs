namespace Xchain.Tests.Templates;

public class WorkflowC : WorkflowChain
{
    protected override void Configure(IWorkflowBuilder b) => b
        .After<FlowA.Step_03_Import>()
        .Start<FlowC.Step_01_Project>();
}
