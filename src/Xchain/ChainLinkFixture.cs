namespace Xchain;

public class ChainLinkFixture : IDisposable
{
    public string _collectionName;
    
    public ChainLinkFixture(string collectionName)
    {
        ChainSync.Register(collectionName);
        _collectionName = collectionName;
    }

    public void Dispose()
    {
        ChainSync.Unregister(_collectionName);
    }
}
