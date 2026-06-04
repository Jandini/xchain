namespace Xchain.Tests.Templates;

// Awaits both completed workflows before running isolation assertions.
[CollectionDefinition("Templates_VerifyIsolation")]
public class VerifyIsolationDefinition :
    ICollectionFixture<CollectionChainAwait<ImportA>>,
    ICollectionFixture<CollectionChainAwait<ImportB>>,
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("Templates_VerifyIsolation")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class VerifyIsolation(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Verify output keys are isolated across workflows")]
    public void VerifyKeyIsolation() =>
        chain.Link(output =>
        {
            Assert.NotEqual(output.ClientId<ClientA>().Key, output.ClientId<ClientB>().Key);
            Assert.NotEqual(output.ProjectId<ProjectA>().Key, output.ProjectId<ProjectB>().Key);
            Assert.NotEqual(output.ImportId<ImportA>().Key, output.ImportId<ImportB>().Key);
        });

    [ChainFact(Link = 2, Name = "Verify output values are isolated across workflows")]
    public void VerifyValueIsolation() =>
        chain.LinkUnless<Exception>(output =>
        {
            Assert.NotEqual(output.ClientId<ClientA>().Get(), output.ClientId<ClientB>().Get());
            Assert.NotEqual(output.ProjectId<ProjectA>().Get(), output.ProjectId<ProjectB>().Get());
            Assert.NotEqual(output.ImportId<ImportA>().Get(), output.ImportId<ImportB>().Get());
        });

    // Proves [TestCaseOrderer] was inherited by CreateClientChain subclasses:
    // CreateClientChain declares VerifyClient (Link=2) before CreateClient (Link=1) in source.
    // If the orderer was not inherited, VerifyClient would run first and fail on the missing key.
    // Getting here with both values populated proves the orderer fired correctly on both instances.
    [ChainFact(Link = 3, Name = "Verify TestCaseOrderer was inherited by both client instances")]
    public void VerifyOrdererInheritance() =>
        chain.LinkUnless<Exception>(output =>
        {
            Assert.True(output.ClientId<ClientA>().ContainsKey(), "ClientA orderer not inherited");
            Assert.True(output.ClientId<ClientB>().ContainsKey(), "ClientB orderer not inherited");
        });
}
