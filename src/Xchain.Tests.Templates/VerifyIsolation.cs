using FlowA = Xchain.Tests.Templates.FlowA;
using FlowB = Xchain.Tests.Templates.FlowB;
using FlowC = Xchain.Tests.Templates.FlowC;

namespace Xchain.Tests.Templates;

// Final collection — runs after all three flows have fully completed.
// Asserts that CRTP output isolation held at runtime across all flows.
//
// Multiple upstream dependencies require inline fixture declarations.
// No SignalFixture needed — nothing awaits VerifyIsolation.
//
//   FlowA.Step_03_Import   ──┐
//   FlowB.Step_03_Import   ──┼──► VerifyIsolation
//   FlowC.Step_01_Project  ──┘
[CollectionDefinition("VerifyIsolation")]
public class VerifyIsolationDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA.Step_03_Import>>,
    ICollectionFixture<CollectionChainAwait<FlowB.Step_03_Import>>,
    ICollectionFixture<CollectionChainAwait<FlowC.Step_01_Project>>,
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("VerifyIsolation")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class VerifyIsolation(CollectionChainContextFixture chain)
{
    // Confirms that the CRTP type parameter + namespace produces structurally different
    // output keys. FlowA.Step_01_Client and FlowB.Step_01_Client are different types
    // (different FullName), so their keys differ even though the class name is identical.
    [ChainFact(Link = 1, Name = "Verify output keys are isolated across all flows")]
    public void VerifyKeyIsolation() =>
        chain.Link(output =>
        {
            Assert.NotEqual(output.ClientId<FlowA.Step_01_Client>().Key,
                            output.ClientId<FlowB.Step_01_Client>().Key);

            Assert.NotEqual(output.ProjectId<FlowA.Step_02_Project>().Key,
                            output.ProjectId<FlowB.Step_02_Project>().Key);

            Assert.NotEqual(output.ProjectId<FlowA.Step_02_Project>().Key,
                            output.ProjectId<FlowC.Step_01_Project>().Key);

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
            var projectC = output.ProjectId<FlowC.Step_01_Project>().Get();
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
