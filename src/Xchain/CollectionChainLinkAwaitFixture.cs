namespace Xchain;

public class CollectionChainLinkAwaitFixture<T>
{   
    public CollectionChainLinkAwaitFixture() => CollectionChainLinkAwaiter.WaitForCollection(typeof(T).Name, TimeSpan.FromSeconds(360));
    public CollectionChainLinkAwaitFixture(TimeSpan timeout) => CollectionChainLinkAwaiter.WaitForCollection(typeof(T).Name, timeout);
}

