namespace Xchain.Tests.Templates;

// FlowB — step 2 of 3.
// Same CreateProjectChain template as FlowA_02_Project, but wired to FlowB_01_Client.
// Output key: output.ProjectId<FlowB_02_Project>() → "...FlowB_02_Project_ProjectId"
[CollectionDefinition("ProjectB")]
public class FlowB_02_ProjectDefinition : CollectionChainNextDefinition<FlowB_01_Client, FlowB_02_Project>;

[Collection("ProjectB")]
public class FlowB_02_Project(CollectionChainContextFixture chain)
    : CreateProjectChain<FlowB_02_Project, FlowB_01_Client>(chain);
