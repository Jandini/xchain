
namespace Xchain.Tests.Templates.FlowB;

// FlowB — step 1 of 3. Identical structure to FlowA.Step_01_Client — same template, different namespace.
// The namespace makes the types distinct: FullName resolves to
//   "Xchain.Tests.Templates.FlowB.Step_01_Client"
// so output keys and awaiter keys are completely isolated from FlowA's Step_01_Client.
// Both flows start in parallel — neither waits for the other.
[CollectionDefinition("ClientB")]
public class Step_01_ClientDefinition : CollectionChainStartDefinition<Step_01_Client>;

[Collection("ClientB")]
public class Step_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<Step_01_Client>(chain);
