namespace Xchain.Tests;

[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest(TestChainFixture testChain) : IClassFixture<TestChainFixture>
{
    [Fact, Link(3)]
    public void Test1()
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)        
        {
            testChain.Errors.Push(ex);
        }

    }

    [Fact, Link(2)]
    public void Test2()
    {
        var sleep = (int)testChain.Output["Sleep"];
        Thread.Sleep(sleep);
    }

    [Fact, Link(1)]
    public void Test3()
    {
        var sleep = 1000;
        Thread.Sleep(sleep);
        testChain.Output["Sleep"] = sleep * 2;
    }
}