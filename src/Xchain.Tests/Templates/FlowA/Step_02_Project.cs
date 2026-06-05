using Xchain.Tests.Templates;

namespace Xchain.Tests.Templates.FlowA;

// FlowA — step 2 of 3. Awaits Step_01_Client, then creates a project for that client.
//
// CollectionChainNextDefinition<TAwait, T>:
//   • Blocks until Step_01_Client signals (TAwait).
//   • Registers Step_02_Project so downstream collections can await it (T).
//   • Includes CollectionChainContextFixture automatically.
[CollectionDefinition("ProjectA")]
public class Step_02_ProjectDefinition : CollectionChainNextDefinition<Step_01_Client, Step_02_Project>;

[Collection("ProjectA")]
public class Step_02_Project(CollectionChainContextFixture chain)
    : CreateProjectChain<Step_02_Project, Step_01_Client>(chain);
