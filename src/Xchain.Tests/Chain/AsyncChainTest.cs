namespace Xchain.Tests;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class AsyncChainTest(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    class FlowFactAttribute : ChainFactAttribute
    {
        public FlowFactAttribute() => Flow = "Async Chain Test";
    }

    [FlowFact(Link = 1, Name = "Async Set Value")]
    public async Task Step1_SetValueAsync() =>
        await chain.LinkAsync(async (output, token) =>
        {
            await Task.Delay(100, token);
            output["Message"] = "Hello from async step";
        });

    [FlowFact(Link = 2, Name = "Async Read Value")]
    public async Task Step2_ReadValueAsync() =>
        await chain.LinkAsync(async (output, token) =>
        {
            await Task.Delay(100, token);
            var message = output.Get<string>("Message");
            Assert.Equal("Hello from async step", message);
        });

    [FlowFact(Link = 3, Name = "Async Exception")]
    public async Task Step3_ThrowsAsync() =>
        await chain.LinkAsync(async (output, token) =>
        {
            await Task.Delay(50, token);
            throw new InvalidOperationException("Expected failure");
        });

    [FlowFact(Link = 4, Name = "Async Skip on Exception")]
    public async Task Step4_SkippedAsync() =>
        await chain.LinkUnlessAsync<InvalidOperationException>(async (output, token) =>
        {
            await Task.Delay(50, token);
            throw new Exception("Should not run");
        });
}
