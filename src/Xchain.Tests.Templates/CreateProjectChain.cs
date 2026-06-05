namespace Xchain.Tests.Templates;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class CreateProjectChain<TSelf, TClient>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create project for client")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<TClient>(), output =>
        {
            var clientId = output.ClientId<TClient>().Get();
            output.ProjectId<TSelf>().Put($"project-for-{clientId}");
        });

    [ChainFact(Link = 2, Name = "Verify project created")]
    public void VerifyProject() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEmpty(output.ProjectId<TSelf>().Get()));
}
