using Xunit.Abstractions;

namespace Xchain;

public class CollectionChainLinkAwaitFixture<TCollection>
{    
    public CollectionChainLinkAwaitFixture() 
        => CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, TimeSpan.FromSeconds(360));
    public CollectionChainLinkAwaitFixture(IMessageSink messageSink)
        => CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, TimeSpan.FromSeconds(360), messageSink);

    public CollectionChainLinkAwaitFixture(TimeSpan timeout) : base()
        => CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, timeout);

    public CollectionChainLinkAwaitFixture(TimeSpan timeout, IMessageSink messageSink) : base()
        => CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, timeout, messageSink);

}

