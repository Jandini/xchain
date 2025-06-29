namespace Xchain;

/// <summary>
/// A shared fixture that enables data and error propagation across multiple test collections.
/// Inherits from <see cref="TestChainContextFixture"/>, but overrides output with a static store,
/// making it accessible across test class boundaries.
/// </summary>
/// <remarks>
/// Use this when chaining tests that span multiple test classes or collections.
/// Keys stored in the shared <see cref="Output"/> must be globally unique across all collections
/// to avoid collisions and data leakage.
/// </remarks>
public class CollectionChainContextFixture : TestChainContextFixture
{
    private static readonly TestChainOutput _output = [];

    /// <summary>
    /// Shared output storage across all test collections using this fixture.
    /// </summary>
    public override TestChainOutput Output => _output;
}
