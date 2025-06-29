namespace Xchain;

/// <summary>
/// Exception thrown when a requested key is not found in the <see cref="TestChainOutput"/> dictionary.
/// Indicates that the expected data has not been produced or shared by earlier tests in the chain.
/// </summary>
/// <remarks>
/// This is typically a test author error — it warns that output was expected from a prior test
/// but was never set, possibly due to test misordering or skipped dependencies.
/// </remarks>
internal class TestChainOutputMissingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the exception with a specific missing output key.
    /// </summary>
    /// <param name="name">The missing key name.</param>
    internal TestChainOutputMissingException(string name)
        : base($"Chain output \"{name}\" is missing or invalid. Ensure this test runs as part of the collection or the output is present in the chain.")
    {
    }
}
