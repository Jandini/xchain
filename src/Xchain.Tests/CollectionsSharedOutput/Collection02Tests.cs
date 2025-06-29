namespace Xchain.Tests.CollectionsSharedOutput;

[CollectionDefinition("Collection02")]
[Metadata("SharedChain")]
public class Collection02 :
    ICollectionFixture<Collection01_Await>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class Collection01_Await : CollectionChainLinkAwaitFixture<Collection01Tests>;

[Collection("Collection02")]
public class Collection02Tests(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Read Shared Id")]
    public void ReadId() =>
        chain.Link(output =>
        {
            var id = output.SharedId<Collection01Tests>().Get();
            Assert.NotEqual(Guid.Empty, id);
        });
}
