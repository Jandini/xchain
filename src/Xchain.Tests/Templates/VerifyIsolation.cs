namespace Xchain.Tests.Templates;

// Final collection — runs after all three chains (FlowA, FlowB, and the cross-flow ProjectC)
// have fully completed. Asserts that the CRTP output isolation actually held at runtime.
//
// Like ProjectC, this has multiple upstream dependencies so it must declare its fixtures inline.
// It does not signal itself (nothing awaits VerifyIsolation), so no SignalFixture is needed.
//
// Execution order:
//   FlowA_03_Import  ──┐
//   FlowB_03_Import  ──┼──► VerifyIsolation
//   ProjectC         ──┘
[CollectionDefinition("VerifyIsolation")]
public class VerifyIsolationDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA_03_Import>>,
    ICollectionFixture<CollectionChainAwait<FlowB_03_Import>>,
    ICollectionFixture<CollectionChainAwait<ProjectC>>,
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("VerifyIsolation")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class VerifyIsolation(CollectionChainContextFixture chain)
{
    // Confirms that the CRTP type parameter produces structurally different output keys.
    // These are pure string comparisons — no execution dependency on other collections.
    [ChainFact(Link = 1, Name = "Verify output keys are isolated across all flows")]
    public void VerifyKeyIsolation() =>
        chain.Link(output =>
        {
            // Client keys differ: FlowA_01_Client vs FlowB_01_Client
            Assert.NotEqual(output.ClientId<FlowA_01_Client>().Key, output.ClientId<FlowB_01_Client>().Key);

            // Project keys differ across all three project instances
            Assert.NotEqual(output.ProjectId<FlowA_02_Project>().Key, output.ProjectId<FlowB_02_Project>().Key);
            Assert.NotEqual(output.ProjectId<FlowA_02_Project>().Key, output.ProjectId<ProjectC>().Key);
            Assert.NotEqual(output.ProjectId<FlowB_02_Project>().Key, output.ProjectId<ProjectC>().Key);

            // Import keys differ: FlowA_03_Import vs FlowB_03_Import
            Assert.NotEqual(output.ImportId<FlowA_03_Import>().Key, output.ImportId<FlowB_03_Import>().Key);
        });

    // Confirms each flow wrote its own independent values.
    // All three chains ran in parallel — their outputs are isolated by type, not by timing.
    [ChainFact(Link = 2, Name = "Verify output values are isolated across all flows")]
    public void VerifyValueIsolation() =>
        chain.LinkUnless<Exception>(output =>
        {
            // FlowA and FlowB clients created different GUIDs
            Assert.NotEqual(output.ClientId<FlowA_01_Client>().Get(), output.ClientId<FlowB_01_Client>().Get());

            // FlowA, FlowB, and cross-flow ProjectC all produced different project records
            var projectA = output.ProjectId<FlowA_02_Project>().Get();
            var projectB = output.ProjectId<FlowB_02_Project>().Get();
            var projectC = output.ProjectId<ProjectC>().Get();
            Assert.NotEmpty(projectA);
            Assert.NotEmpty(projectB);
            Assert.NotEmpty(projectC);
            Assert.NotEqual(projectA, projectB);
            Assert.NotEqual(projectA, projectC);

            // ProjectC combined outputs from both flows
            Assert.Contains("cross-flow-project", projectC);
        });

    // Mechanism check: CreateClientChain declares VerifyClient (Link=2) before CreateClient (Link=1)
    // in source order. If [TestCaseOrderer] was not inherited, VerifyClient would execute first,
    // call .Get() on a key that doesn't exist yet, and throw. Reaching this assertion with both
    // client keys populated proves the orderer fired on the inherited attribute.
    [ChainFact(Link = 3, Name = "Verify TestCaseOrderer was inherited by template subclasses")]
    public void VerifyOrdererInheritance() =>
        chain.LinkUnless<Exception>(output =>
        {
            Assert.True(output.ClientId<FlowA_01_Client>().ContainsKey(), "FlowA orderer not inherited");
            Assert.True(output.ClientId<FlowB_01_Client>().ContainsKey(), "FlowB orderer not inherited");
        });
}
