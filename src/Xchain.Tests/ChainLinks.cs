[assembly: TestCollectionOrderer("Xchain.ChainLinker", "Xchain")]
[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 8)]

namespace Xchain.Tests;

public class LongRunningCollectionFixture() : ChainLinkFixture("WaitForMe");
public class WaitForLongRunningCollectionFixture() : ChainAwaiterFixture("WaitForMe");


[CollectionDefinition("First")]
public class FirstCollection : ICollectionFixture<CollectionChainFixture> { };


[CollectionDefinition("Second")]
public class SecondCollection : ICollectionFixture<WaitForLongRunningCollectionFixture>, ICollectionFixture<CollectionChainFixture> { };

[ChainLink(1)]
[CollectionDefinition("Third")]
public class LinkedCollection : ICollectionFixture<LongRunningCollectionFixture>, ICollectionFixture<CollectionChainFixture> { }

[CollectionDefinition("Four")]
public class LastCollection : ICollectionFixture<WaitForLongRunningCollectionFixture>, ICollectionFixture<CollectionChainFixture> { };

