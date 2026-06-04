using Xchain;
using Xchain.Tests;

[CollectionDefinition("SecondCollection")]
public class SecondCollectionDefinition :
    ICollectionFixture<ProducerAwaitFixture>,
    ICollectionFixture<ConsumerRegisterFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class ProducerAwaitFixture : CollectionChainLinkAwaitFixture<ProducerCollection>;
internal class ConsumerRegisterFixture : CollectionChainLinkSetupFixture<ConsumerCollection>;

[Metadata("Xchain Collection")]
[Collection("SecondCollection")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ConsumerCollection(CollectionChainContextFixture chain) : IClassFixture<CollectionChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Consume Shared Value", Flow = "Collection Chain")]
    public void ConsumeValue() =>
        chain.LinkWithCollection<ProducerCollection>("SharedKey", output =>
        {
            var value = output.Get<string>("SharedKey");
            Assert.Equal("Shared Result", value);
        });
}
