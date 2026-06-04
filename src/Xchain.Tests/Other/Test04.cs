namespace Xchain.Tests.Other;

// "Four" collection demonstrates CollectionChainFixture<TAwait, TRegister> —
// a single fixture that both awaits Test01 AND registers Test04 for any downstream consumers.
// This replaces the two-class pattern (CollectionChainLinkAwaitFixture + CollectionChainLinkSetupFixture).
[CollectionDefinition("Four")]
public class FourCollection :
    ICollectionFixture<CollectionChainFixture<Test01, Test04>>,
    ICollectionFixture<CollectionChainContextFixture>;


[Collection("Four")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class Test04(CollectionChainContextFixture chain)
{
    // Demonstrates: reading cross-collection output works identically whether you used
    // the old two-fixture pattern or the new CollectionChainFixture<TAwait, TRegister>.
    [ChainFact(Link = 1, Name = "Read Sleep value from Test01 (via CollectionChainFixture)")]
    public void LinkedTest1() =>
        chain.LinkWithCollection<Test01>("Sleep", output =>
        {
            var sleep = output.Get<int>("Sleep");
            Assert.Equal(2000, sleep);
        });

    // Demonstrates: static Errors from CollectionChainContextFixture are shared across
    // all collections — Test01's TimeoutException causes this step to skip here too.
    [ChainFact(Link = 2, Name = "Skip because Test01 timed out (shared static Errors)")]
    public void LinkedTest2() =>
        chain.LinkUnless<TimeoutException>(output =>
        {
            Assert.Fail("Should have been skipped — Test01's TimeoutException is in the shared error stack.");
        });
}
