namespace Xchain;

/// <summary>
/// Similar to <see cref="TestChainContextFixture"/>, it tracks errors and output.
/// This one shares output between collections.
/// NOTE: The output keys must be unique across collections.
/// </summary>
public class CollectionChainContextFixture : TestChainContextFixture
{
    private static readonly TestChainOutput _output = [];
    public override TestChainOutput Output => _output;    
}
