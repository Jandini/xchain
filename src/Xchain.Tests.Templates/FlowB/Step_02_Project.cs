
namespace Xchain.Tests.Templates.FlowB;

// FlowB — step 2 of 3.
[CollectionDefinition("ProjectB")]
public class Step_02_ProjectDefinition : CollectionChainNextDefinition<Step_01_Client, Step_02_Project>;

[Collection("ProjectB")]
public class Step_02_Project(CollectionChainContextFixture chain)
    : CreateProjectChain<Step_02_Project, Step_01_Client>(chain);
