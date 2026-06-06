
namespace Xchain.Tests.Templates.FlowA;

// FlowA — step 1 of 3. First collection in the chain — signals completion, does not await anything.
//
// CollectionChainStartDefinition<T> wires up:
//   • CollectionChainSignalFixture<Step_01_Client> — signals "FlowA client done" on disposal.
//   • CollectionChainContextFixture — shared static Output/Errors across all collections.
//
// CRTP: passing itself as TSelf means output keys are automatically namespaced to this type.
//   output.ClientId<Step_01_Client>() → key "Xchain.Tests.Templates.FlowA.Step_01_Client_ClientId"
// The equivalent in FlowB produces a different key, so the two flows never collide.
//
// [TestCaseOrderer] is NOT declared here — it is inherited from CreateClientChain<T>.
[CollectionDefinition("ClientA")]
public class Step_01_ClientDefinition : CollectionChainStartDefinition<Step_01_Client>;

[Collection("ClientA")]
public class Step_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<Step_01_Client>(chain);
