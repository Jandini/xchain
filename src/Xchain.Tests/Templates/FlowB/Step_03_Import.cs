using Xchain.Tests.Templates;

namespace Xchain.Tests.Templates.FlowB;

// FlowB — step 3 of 3. Signals itself so both VerifyIsolation and ProjectC can await its completion.
[CollectionDefinition("ImportB")]
public class Step_03_ImportDefinition : CollectionChainNextDefinition<Step_02_Project, Step_03_Import>;

[Collection("ImportB")]
public class Step_03_Import(CollectionChainContextFixture chain)
    : ImportDataChain<Step_03_Import, Step_02_Project>(chain);
