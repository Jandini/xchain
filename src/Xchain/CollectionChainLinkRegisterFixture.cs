namespace Xchain;

public class CollectionChainLinkRegisterFixture<T> : IDisposable
{
    public CollectionChainLinkRegisterFixture() => CollectionChainLinkAwaiter.Register(typeof(T).Name);
    public void Dispose() => CollectionChainLinkAwaiter.Unregister(typeof(T).Name);
}
