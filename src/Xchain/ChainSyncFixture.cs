using Xunit.Abstractions;

namespace Xchain;

public class ChainSyncFixture
{   
    public ChainSyncFixture(string collectionName) => ChainSync.WaitForCollection(collectionName, TimeSpan.FromSeconds(360));
    public ChainSyncFixture(string collectionName, TimeSpan timeout) => ChainSync.WaitForCollection(collectionName, timeout);
}

