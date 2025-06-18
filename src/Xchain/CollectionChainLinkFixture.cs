namespace Xchain;

public class CollectionChainLinkFixture : IDisposable
{
    public string _collectionName;
    
    public CollectionChainLinkFixture(string collectionName)
    {
        CollectionChainAwaiter.Register(collectionName);
        _collectionName = collectionName;
    }

    public void Dispose()
    {
        CollectionChainAwaiter.Unregister(_collectionName);
    }
}
