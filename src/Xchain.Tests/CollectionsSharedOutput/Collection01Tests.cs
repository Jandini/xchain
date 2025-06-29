namespace Xchain.Tests.CollectionsSharedOutput;

[CollectionDefinition("Collection01")]
[Metadata("SharedChain")]
public class Collection01 :
    ICollectionFixture<Collection01_Register>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class Collection01_Register : CollectionChainLinkSetupFixture<Collection01Tests>;

[Collection("Collection01")]
public class Collection01Tests(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Generate Id")]
    public void GenerateId() =>
        chain.Link(output => output.SharedId<Collection01Tests>().Put(Guid.NewGuid()));
}
