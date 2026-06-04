namespace Xchain.Tests.Templates;

[CollectionDefinition("Templates_FlowA_01_Client")]
public class ClientADefinition : CollectionChainStartDefinition<ClientA>;

[Collection("Templates_FlowA_01_Client")]
public class ClientA(CollectionChainContextFixture chain) : CreateClientChain<ClientA>(chain);
