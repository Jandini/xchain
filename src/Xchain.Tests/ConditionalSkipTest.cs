namespace Xchain.Tests;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ConditionalSkipTest(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Throw exception")]
    public void Step1_Fails() =>
        chain.Link(output =>
        {
            throw new InvalidOperationException("Deliberate failure");
        });

    [ChainFact(Link = 2, Name = "Should be skipped")]
    public void Step2_SkippedIfPreviousFailed() =>
        chain.LinkUnless<InvalidOperationException>(output =>
        {
            // This block should not run if Step1 threw the expected exception
            throw new Exception("This should not execute");
        });
}
