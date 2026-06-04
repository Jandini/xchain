namespace Xchain.Tests.Templates;

// FlowA — step 3 of 3.
// Awaits FlowA_02_Project, then imports data for that project.
// Signals itself so VerifyIsolation can await the completion of the full FlowA chain.
[CollectionDefinition("ImportA")]
public class FlowA_03_ImportDefinition : CollectionChainNextDefinition<FlowA_02_Project, FlowA_03_Import>;

[Collection("ImportA")]
public class FlowA_03_Import(CollectionChainContextFixture chain)
    : ImportDataChain<FlowA_03_Import, FlowA_02_Project>(chain);
