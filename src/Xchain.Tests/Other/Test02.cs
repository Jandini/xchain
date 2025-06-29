namespace Xchain.Tests.Other;

[CollectionDefinition("Second")]
public class SecondCollection : 
    // This collection will start only after FirstTest collection is done
    ICollectionFixture<Test01_CollectionAwaitFixture>, 
    ICollectionFixture<Test03_CollectionRegisterFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

public class Test03_CollectionRegisterFixture() : CollectionChainLinkRegisterFixture<Test03>();

public static class Test01_OutputExtensions 
{ 
    public static TestOutput<Test01, int> Id(this TestChainOutput output) => new(output, "Id");
    public static TestOutput<Test01, int> Id(this TestChainOutput output, out int id)
    {
        var result = output.Id();
        result.TryGet(out id);
        return result;
    }
            
};


[Collection("Second")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class Test02(CollectionChainContextFixture chain)
{

    [ChainFact(Link = 1)]
    public void LinkedTest1() => 
        chain.LinkWithCollection(chain.Output.Id(out var id),
            (output) =>
            {
                chain.Output["x"] = id;
                Thread.Sleep(5000);
                throw new NotImplementedException();
            });


    [ChainFact(Link = 2)]
    public void LinkedTest2() => chain.LinkUnless<Exception>((output) =>
    {
        chain.Output["x"] = 10;
        Thread.Sleep(5000);
        throw new NotImplementedException();
    });

    [ChainFact(Link = 3)]
    public void LinkedTest3() => chain.LinkUnless<Exception>((output) =>
    {
        chain.Output["x"] = 10;
        Thread.Sleep(5000);
    });


    [ChainFact(Link = 4)]
    public void LinkedTest4() => chain.LinkUnless<Exception>((output) =>
    {
        var sleep = output.Get("Sleep");
        Thread.Sleep(5000);
        throw new NotImplementedException(sleep);
    });

    [ChainFact(Link = 5)]
    public void LinkedTest5() => chain.LinkUnless<Exception>((output) =>
    {
        chain.Output["x"] = 10;
        Thread.Sleep(5000);
    });
}