namespace Xchain.Tests;

[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest
{
    [Fact, Link(3)]
    public void Test1()
    {
        Thread.Sleep(1000);
    }

    [Fact, Link(2)]
    public void Test2()
    {
        Thread.Sleep(3000);
    }

    [Fact, Link(1)]
    public void Test3()
    {
        Thread.Sleep(2000);
    }
}