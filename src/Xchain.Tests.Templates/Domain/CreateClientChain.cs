namespace Xchain.Tests.Templates;

// [TestCaseOrderer] is placed on this abstract base so all concrete subclasses inherit it.
// VerifyClient (Link=2) is declared BEFORE CreateClient (Link=1) in source — if the orderer
// is not inherited, xUnit runs VerifyClient first and the Get() throws (key not yet written).
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class CreateClientChain<TSelf>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 2, Name = "Verify client created")]
    public void VerifyClient() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEqual(Guid.Empty, output.ClientId<TSelf>().Get()));

    [ChainFact(Link = 1, Name = "Create client")]
    public void CreateClient() =>
        chain.Link(output => output.ClientId<TSelf>().Put(Guid.NewGuid()));
}
