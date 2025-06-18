
namespace Xchain.Tests;

[CollectionDefinition("Second")]
public class SecondCollection : 
    // This collection will start only after FirstTest collection is done
    ICollectionFixture<Test01_CollectionAwaitFixture>, 
    ICollectionFixture<Test03_CollectionLinkFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

public class Test03_CollectionLinkFixture() : CollectionChainLinkRegisterFixture<Test03>();


[Collection("Second")]
public class Test02(CollectionChainContextFixture chain)
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