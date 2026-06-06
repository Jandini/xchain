namespace Xchain.Tests.Templates;

public class WorkflowA : WorkflowChain
{
    protected override void Configure(IWorkflowBuilder b) => b
        .Start<FlowA.Step_01_Client>()
        .Then<FlowA.Step_02_Project>()
        .End<FlowA.Step_03_Import>();
}
