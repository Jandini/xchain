
namespace Xchain.Tests;


[Collection("Third")]
public class _03_LastTest(CollectionChainFixture chain)
{

    [Fact()]
    public void LinkedTest1() => chain.Link((output) =>
    {
        Thread.Sleep(5000);
        throw new NotImplementedException();
    });


    [Fact()]
    public void LinkedTest22() => chain.Link((output) =>
    {
        Thread.Sleep(5000);
        throw new NotImplementedException();
    });

    [Fact()]
    public void LinkedTest3() => chain.Link((output) =>
    {
        Thread.Sleep(5000);
    });


    [Fact()]
    public void LinkedTest4() => chain.Link((output) =>
    {
        Thread.Sleep(5000);
        throw new NotImplementedException(output.Get("Sleep"));
    });

    [Fact()]
    public void LinkedTest5() => chain.Link((output) =>
    {
        Thread.Sleep(5000);
    });
}