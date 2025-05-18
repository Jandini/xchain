namespace Xchain.Tests;

[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest(TestChainFixture chain) : IClassFixture<TestChainFixture>
{
    [ChainFact, Link(3)]
    public void Test1() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });


    [ChainFact, Link(2)]
    public void Test2() => chain.LinkUnless<NotImplementedException>((output) =>
    {
        var sleep = output.Get<int>("Sleep");
        Thread.Sleep(sleep);
    });
    

    [ChainFact, Link(1)]
    public void Test3() => chain.Link((output) =>
    {
        var sleep = 100;
        Thread.Sleep(sleep);
        output["Sleep"] = sleep * 2;

        throw new TimeoutException();
    });

    [ChainFact, Link(4)]
    public void Test4() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });

    [ChainFact, Link(5)]
    public void Test5() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });
}