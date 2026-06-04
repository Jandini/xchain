namespace Xchain.Tests.Templates;

[CollectionDefinition("Templates_FlowA_02_Project")]
public class ProjectADefinition : CollectionChainNextDefinition<ClientA, ProjectA>;

[Collection("Templates_FlowA_02_Project")]
public class ProjectA(CollectionChainContextFixture chain) : CreateProjectChain<ProjectA, ClientA>(chain);
