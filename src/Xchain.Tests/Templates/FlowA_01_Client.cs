namespace Xchain.Tests.Templates;

// FlowA — step 1 of 3.
// This is the starting collection in Flow A. It creates a client and signals completion
// so that downstream collections (FlowA_02_Project and ProjectC) can unblock.
//
// CollectionChainStartDefinition<T> wires up two fixtures automatically:
//   • CollectionChainSignalFixture<FlowA_01_Client> — registers this collection in the
//     awaiter and signals "done" when the fixture is disposed (after all tests finish).
//   • CollectionChainContextFixture — provides the shared static Output/Errors state.
//
// The [CollectionDefinition] name ("ClientA") is just an identifier string. It is
// independent from the class name — the class name is what controls ordering in
// Visual Studio Test Explorer (alphabetical sort by class name).
[CollectionDefinition("ClientA")]
public class FlowA_01_ClientDefinition : CollectionChainStartDefinition<FlowA_01_Client>;

// The test class passes itself as TSelf (CRTP pattern). This means every output key
// written here is namespaced to this specific type:
//   output.ClientId<FlowA_01_Client>() → key "...FlowA_01_Client_ClientId"
//
// If FlowB_01_Client runs the same template, it writes to a different key:
//   output.ClientId<FlowB_01_Client>() → key "...FlowB_01_Client_ClientId"
//
// No collisions, no coordination needed — the isolation is structural.
//
// [TestCaseOrderer] is NOT repeated here because CreateClientChain<T> already declares it.
// xUnit inherits attributes with Inherited=true, so all subclasses pick it up automatically.
[Collection("ClientA")]
public class FlowA_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<FlowA_01_Client>(chain);
