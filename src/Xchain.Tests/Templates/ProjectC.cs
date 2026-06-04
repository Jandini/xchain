namespace Xchain.Tests.Templates;

// Cross-flow collection — demonstrates fan-in: a single collection that depends on
// outputs from two different flows.
//
// ProjectC needs:
//   • FlowA_01_Client — to read the client credentials created in Flow A
//   • FlowB_03_Import — to read the import result from the end of Flow B
//
// Because there are TWO upstream dependencies, the definition base classes
// (CollectionChainStartDefinition, CollectionChainNextDefinition) cannot be used —
// they each accept only one upstream type. The fixtures are declared inline instead.
//
// Execution: ProjectC unblocks only after BOTH upstreams have signalled completion.
// FlowA_01_Client finishes early (step 1 of FlowA), but FlowB_03_Import finishes last
// in FlowB (step 3). In practice ProjectC starts after the entire FlowB chain completes.
//
//   FlowA: FlowA_01_Client ───────────────────────────────────────────────┐
//                                                                         ▼
//   FlowB: FlowB_01_Client ──► FlowB_02_Project ──► FlowB_03_Import ──► ProjectC
//
[CollectionDefinition("ProjectC")]
public class ProjectCDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA_01_Client>>,   // unblocks when FlowA client is done
    ICollectionFixture<CollectionChainAwait<FlowB_03_Import>>,   // unblocks when FlowB import is done
    ICollectionFixture<CollectionChainSignalFixture<ProjectC>>,  // signals self so VerifyIsolation can await
    ICollectionFixture<CollectionChainContextFixture>;           // shared static Output/Errors

[Collection("ProjectC")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ProjectC(CollectionChainContextFixture chain)
{
    // Reads output from two different flows and combines them into a single project record.
    // LinkWithCollection checks that the ClientA key exists before executing — if FlowA_01_Client
    // somehow failed to write it, this step skips cleanly instead of throwing.
    // The ImportB key is read directly from the shared Output dictionary (no extra check needed
    // because the definition already guarantees FlowB_03_Import has completed before we get here).
    [ChainFact(Link = 1, Name = "Create cross-flow project from ClientA and ImportB")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<FlowA_01_Client>(), output =>
        {
            var clientId = output.ClientId<FlowA_01_Client>().Get();
            var importId = output.ImportId<FlowB_03_Import>().Get();
            output.ProjectId<ProjectC>().Put($"cross-flow-project|client={clientId}|import={importId}");
        });

    // Verifies the combined project record was written correctly.
    // LinkUnless<Exception> skips automatically if any prior step in any collection
    // pushed an exception to the shared Errors stack.
    [ChainFact(Link = 2, Name = "Verify cross-flow project was created")]
    public void VerifyProject() =>
        chain.LinkUnless<Exception>(output =>
        {
            var projectId = output.ProjectId<ProjectC>().Get();
            Assert.Contains("cross-flow-project", projectId);
        });
}
