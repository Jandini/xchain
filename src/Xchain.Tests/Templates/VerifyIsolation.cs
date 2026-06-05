using FlowA = Xchain.Tests.Templates.FlowA;
using FlowB = Xchain.Tests.Templates.FlowB;

namespace Xchain.Tests.Templates;

// Final collection — runs after all three chains (FlowA, FlowB, and cross-flow ProjectC)
// have fully completed. Asserts that the CRTP output isolation held at runtime.
//
// Multiple upstream dependencies require inline fixture declarations (same reason as ProjectC).
// No SignalFixture needed — nothing awaits VerifyIsolation.
//
//   FlowA.Step_03_Import ──┐
//   FlowB.Step_03_Import ──┼──► VerifyIsolation
//   ProjectC             ──┘
[CollectionDefinition("VerifyIsolation")]
public class VerifyIsolationDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA.Step_03_Import>>,
    ICollectionFixture<CollectionChainAwait<FlowB.Step_03_Import>>,
    ICollectionFixture<CollectionChainAwait<ProjectC>>,
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("VerifyIsolation")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class VerifyIsolation(CollectionChainContextFixture chain)
{
    // Confirms that the CRTP type parameter + namespace produces structurally different
    // output keys. These are pure string comparisons — no execution dependency.
    // FlowA.Step_01_Client and FlowB.Step_01_Client are different types (different FullName),
    // so their keys differ even though the class name is identical.
    [ChainFact(Link = 1, Name = "Verify output keys are isolated across all flows")]
    public void VerifyKeyIsolation() =>
        chain.Link(output =>
        {
            Assert.NotEqual(output.ClientId<FlowA.Step_01_Client>().Key,
                            output.ClientId<FlowB.Step_01_Client>().Key);

            Assert.NotEqual(output.ProjectId<FlowA.Step_02_Project>().Key,
                            output.ProjectId<FlowB.Step_02_Project>().Key);

            Assert.NotEqual(output.ProjectId<FlowA.Step_02_Project>().Key,
                            output.ProjectId<ProjectC>().Key);

            Assert.NotEqual(output.ImportId<FlowA.Step_03_Import>().Key,
                            output.ImportId<FlowB.Step_03_Import>().Key);
        });

    // Confirms each flow wrote independent values into those isolated keys.
    [ChainFact(Link = 2, Name = "Verify output values are isolated across all flows")]
    public void VerifyValueIsolation() =>
        chain.LinkUnless<Exception>(output =>
        {
            Assert.NotEqual(output.ClientId<FlowA.Step_01_Client>().Get(),
                            output.ClientId<FlowB.Step_01_Client>().Get());

            var projectA = output.ProjectId<FlowA.Step_02_Project>().Get();
            var projectB = output.ProjectId<FlowB.Step_02_Project>().Get();
            var projectC = output.ProjectId<ProjectC>().Get();
            Assert.NotEmpty(projectA);
            Assert.NotEmpty(projectB);
            Assert.NotEmpty(projectC);
            Assert.NotEqual(projectA, projectB);
            Assert.NotEqual(projectA, projectC);
            Assert.Contains("cross-flow-project", projectC);
        });

    // Mechanism check: CreateClientChain declares VerifyClient (Link=2) before CreateClient
    // (Link=1) in source order. If [TestCaseOrderer] was not inherited, VerifyClient would
    // execute first, call .Get() on a missing key, and throw. Both keys being populated here
    // proves the orderer fired on the inherited attribute in both flows.
    [ChainFact(Link = 3, Name = "Verify TestCaseOrderer was inherited by template subclasses")]
    public void VerifyOrdererInheritance() =>
        chain.LinkUnless<Exception>(output =>
        {
            Assert.True(output.ClientId<FlowA.Step_01_Client>().ContainsKey(), "FlowA orderer not inherited");
            Assert.True(output.ClientId<FlowB.Step_01_Client>().ContainsKey(), "FlowB orderer not inherited");
        });
}
