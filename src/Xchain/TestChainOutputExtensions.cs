namespace Xchain;

public static class TestChainOutputExtensions
{
    /// <summary>
    /// Retrieves a value from the test chain output by name or throws if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The expected type of the output value.</typeparam>
    /// <param name="output">The test chain output.</param>
    /// <param name="name">The key name of the output item.</param>
    /// <returns>The value cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="TestChainOutputMissingException">Thrown if the named output is missing.</exception>
    public static T Get<T>(this TestChainOutput output, string name) =>
        output.ContainsKey(name) ? (T)output[name] : throw new TestChainOutputMissingException(name);


    /// <summary>
    /// Retrieves a value from the test chain output by name and returns it as a string.
    /// Throws if the key does not exist.
    /// </summary>
    /// <param name="output">The test chain output.</param>
    /// <param name="name">The key name of the output item.</param>
    /// <returns>The string representation of the value.</returns>
    /// <exception cref="TestChainOutputMissingException">Thrown if the named output is missing.</exception>
    public static string Get(this TestChainOutput output, string name) =>
        output.ContainsKey(name)
            ? output[name]?.ToString() ?? string.Empty
            : throw new TestChainOutputMissingException(name);

}
