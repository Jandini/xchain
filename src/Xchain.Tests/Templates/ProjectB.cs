namespace Xchain.Tests.Templates;

[CollectionDefinition("Templates_FlowB_02_Project")]
public class ProjectBDefinition : CollectionChainNextDefinition<ClientB, ProjectB>;

[Collection("Templates_FlowB_02_Project")]
public class ProjectB(CollectionChainContextFixture chain) : CreateProjectChain<ProjectB, ClientB>(chain);
