// Namespace aliases resolve the ambiguity: both FlowA and FlowB contain Step_01_Client,
// Step_02_Project, Step_03_Import. The alias makes references like FlowA.Step_01_Client
// unambiguous and self-documenting at the call site.
using FlowA = Xchain.Tests.Templates.FlowA;
using FlowB = Xchain.Tests.Templates.FlowB;

namespace Xchain.Tests.Templates;

// Cross-flow collection — demonstrates fan-in: a single collection that depends on
// outputs from two different flows.
//
// ProjectC needs:
//   • FlowA.Step_01_Client — to read the client credentials created in Flow A.
//   • FlowB.Step_03_Import — to read the import result from the end of Flow B.
//
// Because there are TWO upstream dependencies, the definition base classes
// (CollectionChainStartDefinition, CollectionChainNextDefinition) cannot be used —
// they each accept only one upstream type. The fixtures are declared inline instead.
//
// Execution: ProjectC unblocks only after BOTH upstreams have signalled completion.
// FlowA.Step_01_Client finishes early (step 1 of FlowA), but FlowB.Step_03_Import
// finishes last in FlowB (step 3). In practice ProjectC starts after the entire
// FlowB chain completes.
//
//   FlowA: Step_01_Client ─────────────────────────────────────────────────────┐
//                                                                              ▼
//   FlowB: Step_01_Client ──► Step_02_Project ──► Step_03_Import ──► ProjectC
//
[CollectionDefinition("ProjectC")]
public class ProjectCDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA.Step_01_Client>>,   // unblocks when FlowA client is done
    ICollectionFixture<CollectionChainAwait<FlowB.Step_03_Import>>,   // unblocks when FlowB import is done
    ICollectionFixture<CollectionChainSignalFixture<ProjectC>>,       // signals self so VerifyIsolation can await
    ICollectionFixture<CollectionChainContextFixture>;                 // shared static Output/Errors

[Collection("ProjectC")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ProjectC(CollectionChainContextFixture chain)
{
    // Reads output from two different flows and combines them into a single project record.
    // LinkWithCollection checks that the FlowA client key exists before executing — if
    // FlowA.Step_01_Client somehow failed to write it, this step skips cleanly.
    // The FlowB import key is read directly; the definition already guarantees
    // FlowB.Step_03_Import has completed before this collection starts.
    [ChainFact(Link = 1, Name = "Create cross-flow project from FlowA client and FlowB import")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<FlowA.Step_01_Client>(), output =>
        {
            var clientId = output.ClientId<FlowA.Step_01_Client>().Get();
            var importId = output.ImportId<FlowB.Step_03_Import>().Get();
            output.ProjectId<ProjectC>().Put($"cross-flow-project|client={clientId}|import={importId}");
        });

    [ChainFact(Link = 2, Name = "Verify cross-flow project was created")]
    public void VerifyProject() =>
        chain.LinkUnless<Exception>(output =>
        {
            var projectId = output.ProjectId<ProjectC>().Get();
            Assert.Contains("cross-flow-project", projectId);
        });
}
