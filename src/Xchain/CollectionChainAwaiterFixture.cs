namespace Xchain;

public class CollectionChainAwaiterFixture
{   
    public CollectionChainAwaiterFixture(string collectionName) => CollectionChainAwaiter.WaitForCollection(collectionName, TimeSpan.FromSeconds(360));
    public CollectionChainAwaiterFixture(string collectionName, TimeSpan timeout) => CollectionChainAwaiter.WaitForCollection(collectionName, timeout);
}

