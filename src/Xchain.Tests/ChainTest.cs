namespace Xchain.Tests;

[TestCaseOrderer("Xchain.ChainOrderer", "Xchain")]
public class ChainTest(TestChainFixture chain) : IClassFixture<TestChainFixture>
{
    [ChainFact(Link = 3, Name = "Throw Exception")]
    public void Test1() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });


    [ChainFact(Link = 2, Name = "Sleep 2 seconds")]
    public async Task Test2() => await chain.LinkUnlessAsync<NotImplementedException>(async (output, cancellationToken) =>
    {
        var sleep = output.Get<int>("Sleep");
        await Task.Delay(sleep, cancellationToken);
    });
    

    [ChainFact(Link = 1, Name = "Sleep 1 second")]
    [ChainTag(Owner = "Kethoneinuo", Category = "Important", Color = "Black")]
    public async Task Test3() => await chain.LinkAsync(async (output, cancellationToken) =>
    {
        const int sleep = 1000;
        output["Sleep"] = sleep * 2;
        await Task.Delay(sleep, cancellationToken);
    }, TimeSpan.FromMilliseconds(100));

    [ChainFact(Link = 4, Name = "Throw Exception")]
    public void Test4() => chain.LinkUnless<Exception>((output) =>
    {
        var sleep = output.Get("Sleep");
        throw new NotImplementedException($"Partially implemented sleep of {sleep} ms");
    });

    [ChainFact(Link = 5, Name = "Throw Exception")]
    public void Test5() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });
}