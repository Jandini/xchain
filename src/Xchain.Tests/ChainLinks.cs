namespace Xchain.Tests;

[CollectionDefinition("ChainTest")]
[ChainLink(1)]
public class ChainCollection : ICollectionFixture<CollectionChainFixture> { };


[CollectionDefinition("LinkedTest")]
[ChainLink(2)]
public class LinkedCollection : ICollectionFixture<CollectionChainFixture> { };


[CollectionDefinition("LastTest")]
[ChainLink(3)]
public class LastCollection : ICollectionFixture<CollectionChainFixture> { };

