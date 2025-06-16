namespace Xchain;

public class ChainLinkFixture : IDisposable
{
    public string _collectionName;
    
    public ChainLinkFixture(string collectionName)
    {
        ChainAwaiter.Register(collectionName);
        _collectionName = collectionName;
    }

    public void Dispose()
    {
        ChainAwaiter.Unregister(_collectionName);
    }
}
