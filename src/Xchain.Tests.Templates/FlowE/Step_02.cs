using Microsoft.Extensions.DependencyInjection;

namespace Xchain.Tests.Templates.FlowE;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public partial class Step_02(CollectionChainContextFixture chain, FlowEFixture fixture)
{
    [ChainFact(Link = 1, Name = "Verify workflow fixture is alive across collection boundary")]
    public void VerifyOutput() =>
        chain.LinkUnless<Exception>(output =>
        {
            Assert.Equal("flow-e", (string)output["flow-e-step1"]!);
            // Same static provider as Step_01 — WithWorkflowFixture keeps it alive.
            Assert.Equal("flow-e", fixture.Services.GetRequiredService<string>());
        });
}
