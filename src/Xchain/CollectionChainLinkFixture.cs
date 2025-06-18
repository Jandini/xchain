namespace Xchain;

public class CollectionChainLinkFixture : IDisposable
{
    public string _collectionName;
    
    public CollectionChainLinkFixture(string collectionName)
    {
        CollectionChainLinkAwaiter.Register(collectionName);
        _collectionName = collectionName;
    }

    public void Dispose()
    {
        CollectionChainLinkAwaiter.Unregister(_collectionName);
    }
}
