namespace Xchain;

public class CollectionChainLinkAwaitFixture
{   
    public CollectionChainLinkAwaitFixture(string collectionName) => CollectionChainLinkAwaiter.WaitForCollection(collectionName, TimeSpan.FromSeconds(360));
    public CollectionChainLinkAwaitFixture(string collectionName, TimeSpan timeout) => CollectionChainLinkAwaiter.WaitForCollection(collectionName, timeout);
}

