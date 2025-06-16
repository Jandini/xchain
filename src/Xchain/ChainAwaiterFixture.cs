namespace Xchain;

public class ChainAwaiterFixture
{   
    public ChainAwaiterFixture(string collectionName) => ChainAwaiter.WaitForCollection(collectionName, TimeSpan.FromSeconds(360));
    public ChainAwaiterFixture(string collectionName, TimeSpan timeout) => ChainAwaiter.WaitForCollection(collectionName, timeout);
}

