namespace Xchain.Tests.Templates;

[CollectionDefinition("Templates_FlowB_03_Import")]
public class ImportBDefinition : CollectionChainNextDefinition<ProjectB, ImportB>;

[Collection("Templates_FlowB_03_Import")]
public class ImportB(CollectionChainContextFixture chain) : ImportDataChain<ImportB, ProjectB>(chain);
