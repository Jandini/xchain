namespace Xchain;

public class CollectionChainLinkFixture<T> : IDisposable
{
    public CollectionChainLinkFixture() => CollectionChainLinkAwaiter.Register(typeof(T).Name);
    public void Dispose() => CollectionChainLinkAwaiter.Unregister(typeof(T).Name);
}
