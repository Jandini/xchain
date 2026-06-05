using Xchain.Tests.Templates;

namespace Xchain.Tests.Templates.FlowA;

// FlowA — step 3 of 3. Awaits Step_02_Project, then imports data for that project.
// Signals itself so VerifyIsolation can await the completion of the full FlowA chain.
[CollectionDefinition("ImportA")]
public class Step_03_ImportDefinition : CollectionChainNextDefinition<Step_02_Project, Step_03_Import>;

[Collection("ImportA")]
public class Step_03_Import(CollectionChainContextFixture chain)
    : ImportDataChain<Step_03_Import, Step_02_Project>(chain);
