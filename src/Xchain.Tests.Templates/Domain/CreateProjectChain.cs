using Xchain.Tests.Templates.Domain;

namespace Xchain.Tests.Templates;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class CreateProjectChain<TSelf, TClient>(CollectionChainContextFixture chain)
{

    [CreateProjectStep(Link = 1, Name = "Create project for client")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<TClient>(), output =>
        {
            var clientId = output.ClientId<TClient>().Get();
            output.ProjectId<TSelf>().Put($"project-for-{clientId}");
        });

    [CreateProjectStep(Link = 2, Name = "Fail test", Skip = "Do not throw")]
    public void FailTest() =>
        chain.LinkUnless<Exception>(_ => throw new Exception("This test is meant to fail to demonstrate LinkWithCollectionUnless"));

    [CreateProjectStep(Link = 3, Name = "Verify project created")]
    public void VerifyProject() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEmpty(output.ProjectId<TSelf>().Get()));
}
