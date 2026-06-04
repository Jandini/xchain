namespace Xchain.Tests.Templates;

// FlowB — step 1 of 3.
// Identical structure to FlowA_01_Client — same template, different type.
// Because the class name is different, TSelf resolves to FlowB_01_Client, and all
// output keys are automatically isolated:
//   output.ClientId<FlowB_01_Client>() → "...FlowB_01_Client_ClientId"
// FlowA and FlowB run in parallel from the start — neither waits for the other.
[CollectionDefinition("ClientB")]
public class FlowB_01_ClientDefinition : CollectionChainStartDefinition<FlowB_01_Client>;

[Collection("ClientB")]
public class FlowB_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<FlowB_01_Client>(chain);
