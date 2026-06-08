namespace Xchain.Tests.Templates;

public class WorkflowB : WorkflowChain
{
    protected override void Configure(IWorkflowBuilder b) => b
        .Start<FlowB.Step_01_Client>()
        .Then<FlowB.Step_02_Project>()
        .End<FlowB.Step_03_Import>();
}
