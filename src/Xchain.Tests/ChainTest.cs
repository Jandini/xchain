namespace Xchain.Tests;

[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest(TestChainFixture testChain) : IClassFixture<TestChainFixture>
{
    [ChainFact, Link(3)]
    public void Test1()
    {
        testChain.LinkUnless<Exception>((output) =>
        {
            throw new NotImplementedException();
        });
    }

    [ChainFact, Link(2)]
    public void Test2()
    {
        testChain.LinkUnless<NotImplementedException>((output) =>
        {
            var sleep = output.Get<int>("Sleep");
            Thread.Sleep(sleep);
        });
    }

    [ChainFact, Link(1)]
    public void Test3()
    {        
        testChain.Link((output) =>
        {
            var sleep = 1000;
            Thread.Sleep(sleep);
            output["Sleep"] = sleep * 2;

            throw new TimeoutException();
        });
    }
}