using Microsoft.Extensions.DependencyInjection;

namespace Xchain.Tests.Templates.FlowE;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public partial class Step_01(CollectionChainContextFixture chain, FlowEFixture fixture)
{
    [ChainFact(Link = 1, Name = "Write to shared output via workflow fixture")]
    public void WriteOutput() =>
        chain.Link(output => output["flow-e-step1"] = fixture.Services.GetRequiredService<string>());
}
