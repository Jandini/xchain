namespace Xchain.Tests.Templates;

// FlowB — step 3 of 3.
// Same ImportDataChain template as FlowA_03_Import, but wired to FlowB_02_Project.
// Signals itself so both VerifyIsolation and ProjectC can await its completion.
[CollectionDefinition("ImportB")]
public class FlowB_03_ImportDefinition : CollectionChainNextDefinition<FlowB_02_Project, FlowB_03_Import>;

[Collection("ImportB")]
public class FlowB_03_Import(CollectionChainContextFixture chain)
    : ImportDataChain<FlowB_03_Import, FlowB_02_Project>(chain);
