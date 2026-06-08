namespace Xchain.Tests.Templates;

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class ImportDataChain<TSelf, TProject>(CollectionChainContextFixture chain, string importName)
{
    [ChainFact(Link = 1, Name = "Import data for project")]
    public void ImportData() =>
        chain.LinkWithCollectionUnless<Exception, TProject>(chain.Output.ProjectId<TProject>().Key, output =>
        {

            var projectId = output.ProjectId<TProject>().Get();
            output.ImportId<TSelf>().Put($"import-for-{projectId}");
            output.ImportName<TSelf>().Put(importName);
        });

    [ChainFact(Link = 2, Name = "Verify import completed")]
    public void VerifyImport() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEmpty(output.ImportId<TSelf>().Get()));
}
