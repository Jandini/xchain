namespace Xchain.Tests.Templates;

// FlowA — step 2 of 3.
// Awaits FlowA_01_Client, then creates a project using that client's output.
//
// CollectionChainNextDefinition<TAwait, T> wires up:
//   • CollectionChainNextFixture<FlowA_01_Client, FlowA_02_Project> — blocks fixture
//     construction until FlowA_01_Client signals completion, then registers this
//     collection so further downstream collections can await it.
//   • CollectionChainContextFixture — shared static Output/Errors.
//
// The two type parameters must agree with the template:
//   Definition awaits:   FlowA_01_Client  (TAwait)
//   Template reads from: FlowA_01_Client  (TClient)
// There is no compiler check — by convention they must match.
[CollectionDefinition("ProjectA")]
public class FlowA_02_ProjectDefinition : CollectionChainNextDefinition<FlowA_01_Client, FlowA_02_Project>;

[Collection("ProjectA")]
public class FlowA_02_Project(CollectionChainContextFixture chain)
    : CreateProjectChain<FlowA_02_Project, FlowA_01_Client>(chain);
