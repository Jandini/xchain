// Namespace aliases resolve the ambiguity between FlowA and FlowB types that share
// identical class names (Step_01_Client, Step_02_Project, Step_03_Import).
using FlowA = Xchain.Tests.Templates.FlowA;
using FlowB = Xchain.Tests.Templates.FlowB;
using Xchain.Tests.Templates;

namespace Xchain.Tests.Templates.FlowC;

// FlowC — step 1. Cross-flow collection that fans in from two independent chains.
//
// This step has no dedicated client of its own — it reuses the client created in FlowA
// and waits for the import from FlowB before creating a cross-flow project.
//
// Because there are TWO upstream dependencies, the definition base classes
// (CollectionChainStartDefinition, CollectionChainNextDefinition) cannot be used —
// they each accept only one upstream type. Fixtures are declared inline instead.
//
// Execution: unblocks only after BOTH upstreams have signalled completion.
// FlowA.Step_01_Client finishes early, but FlowB.Step_03_Import finishes last in FlowB.
// In practice this step starts after the entire FlowB chain completes.
//
//   FlowA: Step_01_Client ──────────────────────────────────────────────────────────┐
//                                                                                   ▼
//   FlowB: Step_01_Client ──► Step_02_Project ──► Step_03_Import ──► FlowC: Step_01_Project
//
[CollectionDefinition("FlowC_Project")]
public class Step_01_ProjectDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA.Step_01_Client>>,    // unblocks when FlowA client is done
    ICollectionFixture<CollectionChainAwait<FlowB.Step_03_Import>>,    // unblocks when FlowB import is done
    ICollectionFixture<CollectionChainSignalFixture<Step_01_Project>>, // signals self so VerifyIsolation can await
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("FlowC_Project")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class Step_01_Project(CollectionChainContextFixture chain)
{
    // Reads output from two different flows and combines them into a single project record.
    // LinkWithCollection checks that the FlowA client key exists before executing.
    // The FlowB import key is read directly — the definition already guarantees
    // FlowB.Step_03_Import has completed before this collection starts.
    [ChainFact(Link = 1, Name = "Create cross-flow project from FlowA client and FlowB import")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<FlowA.Step_01_Client>(), output =>
        {
            var clientId = output.ClientId<FlowA.Step_01_Client>().Get();
            var importId = output.ImportId<FlowB.Step_03_Import>().Get();
            output.ProjectId<Step_01_Project>().Put($"cross-flow-project|client={clientId}|import={importId}");
        });

    [ChainFact(Link = 2, Name = "Verify cross-flow project was created")]
    public void VerifyProject() =>
        chain.LinkUnless<Exception>(output =>
        {
            var projectId = output.ProjectId<Step_01_Project>().Get();
            Assert.Contains("cross-flow-project", projectId);
        });
}
