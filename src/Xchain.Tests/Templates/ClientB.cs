namespace Xchain.Tests.Templates;

[CollectionDefinition("Templates_FlowB_01_Client")]
public class ClientBDefinition : CollectionChainStartDefinition<ClientB>;

[Collection("Templates_FlowB_01_Client")]
public class ClientB(CollectionChainContextFixture chain) : CreateClientChain<ClientB>(chain);
