using Xchain;
using Xchain.Tests;

[CollectionDefinition("FirstCollection")]
public class FirstCollectionDefinition :
    ICollectionFixture<ProducerSignalFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class ProducerSignalFixture : CollectionChainSignalFixture<ProducerCollection>;

[Metadata("Xchain Collection")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
[Collection("FirstCollection")]
public class ProducerCollection(CollectionChainContextFixture chain) 
{
    [ChainFact(Link = 1, Name = "Produce Shared Value", Flow = "Collection Chain")]
    public void ProduceValue() =>
        chain.Link(output => output["SharedKey"] = "Shared Result");
}
