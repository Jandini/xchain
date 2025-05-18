namespace Xchain;

internal class TestChainOutputMissingException : Exception
{
    internal TestChainOutputMissingException(string name)
        : base($"Chain output \"{name}\" is missing or invalid. Ensure this test runs as part of the collection or the output is present in the chain.")
    {
    }
}