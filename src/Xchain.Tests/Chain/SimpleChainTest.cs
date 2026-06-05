namespace Xchain.Tests;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class SimpleChainTest(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Set value")]
    public void Step1_SetValue() =>
        chain.Link(output => output["Answer"] = 42);

    [ChainFact(Link = 2, Name = "Read value")]
    public void Step2_ReadValue() =>
        chain.Link(output =>
        {
            var result = output.Get<int>("Answer");
            Assert.Equal(42, result);
        });
}
