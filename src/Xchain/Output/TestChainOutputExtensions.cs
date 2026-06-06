namespace Xchain;

/// <summary>
/// Extension methods for simplified and type-safe access to values in <see cref="TestChainOutput"/>.
/// </summary>
public static class TestChainOutputExtensions
{
    /// <summary>
    /// Retrieves a value of the specified type from the test chain output by key.
    /// </summary>
    /// <typeparam name="T">The expected type of the output value.</typeparam>
    /// <param name="output">The shared output dictionary.</param>
    /// <param name="name">The key used to retrieve the value.</param>
    /// <returns>The value cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="TestChainOutputMissingException">
    /// Thrown if the key does not exist in the output dictionary.
    /// </exception>
    public static T Get<T>(this TestChainOutput output, string name) =>
        output.ContainsKey(name)
            ? (T)output[name]
            : throw new TestChainOutputMissingException(name);

    /// <summary>
    /// Retrieves a value as a string from the test chain output by key.
    /// </summary>
    /// <param name="output">The shared output dictionary.</param>
    /// <param name="name">The key used to retrieve the value.</param>
    /// <returns>The string representation of the stored object, or an empty string if null.</returns>
    /// <exception cref="TestChainOutputMissingException">
    /// Thrown if the key does not exist in the output dictionary.
    /// </exception>
    public static string Get(this TestChainOutput output, string name) =>
        output.ContainsKey(name)
            ? output[name]?.ToString() ?? string.Empty
            : throw new TestChainOutputMissingException(name);
}
