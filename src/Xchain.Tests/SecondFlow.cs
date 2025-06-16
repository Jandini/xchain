
namespace Xchain.Tests;


[Collection("Second")]
public class _02_SecondFlow(CollectionChainFixture chain)
{


    [Fact()]
    public void LinkedTest1() => chain.Link((output) =>
    {
        chain.Output["x"] = 10;
        Thread.Sleep(5000);
        throw new NotImplementedException();
    });


    [Fact()]
    public void LinkedTest2() => chain.Link((output) =>
    {
        chain.Output["x"] = 10;
        Thread.Sleep(5000);
        throw new NotImplementedException();
    });

    [Fact()]
    public void LinkedTest3() => chain.Link((output) =>
    {
        chain.Output["x"] = 10;
        Thread.Sleep(5000);
    });


    [Fact()]
    public void LinkedTest4() => chain.Link((output) =>
    {
        var sleep = output.Get("Sleep");
        Thread.Sleep(5000);
        throw new NotImplementedException(sleep);
    });

    [Fact()]
    public void LinkedTest5() => chain.Link((output) =>
    {
        chain.Output["x"] = 10;
        Thread.Sleep(5000);
    });
}