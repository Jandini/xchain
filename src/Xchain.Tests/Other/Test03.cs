namespace Xchain.Tests.Other;

// "Third" collection awaits Test01 via Test01_CollectionAwaitFixture (defined in Test01.cs).
// It uses CollectionChainContextFixture whose Output and Errors are both static, so
// Test01's shared output AND any errors from Test01 are visible here.
[CollectionDefinition("Third")]
public class ThirdCollection :
    ICollectionFixture<Test01_CollectionAwaitFixture>,
    ICollectionFixture<CollectionChainContextFixture>;


[Collection("Third")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class Test03(CollectionChainContextFixture chain)
{
    // Demonstrates: reading output produced by a prior collection via LinkWithCollection.
    // LinkWithCollection validates the key exists before executing — no error, just skip if missing.
    [ChainFact(Link = 1, Name = "Read Sleep value from Test01")]
    public void LinkedTest1() =>
        chain.LinkWithCollection<Test01>("Sleep", output =>
        {
            var sleep = output.Get<int>("Sleep");
            Assert.Equal(2000, sleep);
        });

    // Demonstrates: LinkUnless<TException> skips this step because Test01 pushed a
    // TimeoutException into the shared static Errors stack.
    [ChainFact(Link = 2, Name = "Skip because Test01 timed out")]
    public void LinkedTest2() =>
        chain.LinkUnless<TimeoutException>(output =>
        {
            Assert.Fail("Should have been skipped — Test01's TimeoutException is in the shared error stack.");
        });
}
