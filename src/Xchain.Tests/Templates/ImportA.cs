namespace Xchain.Tests.Templates;

[CollectionDefinition("Templates_FlowA_03_Import")]
public class ImportADefinition : CollectionChainNextDefinition<ProjectA, ImportA>;

[Collection("Templates_FlowA_03_Import")]
public class ImportA(CollectionChainContextFixture chain) : ImportDataChain<ImportA, ProjectA>(chain);
