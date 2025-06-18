namespace Xchain;

/// <summary>
/// Provides a convenient wrapper for accessing and storing values in a TestChainOutput
/// using a consistent key pattern based on the type name and an optional suffix.
/// </summary>
public class TestOutput<T>(TestChainOutput output, string suffix = null)
{
    /// <summary>
    /// Retrieves a value of type <typeparamref name="K"/> from the output dictionary
    /// using a key composed of the type name <typeparamref name="T"/> and optional suffix.
    /// </summary>
    /// <typeparam name="K">The type of the value to retrieve.</typeparam>
    /// <returns>The value retrieved from the output dictionary.</returns>
    public K Get<K>() => output.Get<K>($"{typeof(T).Name}{(suffix != null ? $"_{suffix}" : string.Empty)}");

    /// <summary>
    /// Stores the given value in the output dictionary using a key composed
    /// of the type name <typeparamref name="T"/> and optional suffix.
    /// </summary>
    /// <param name="value">The value to store in the output dictionary.</param>
    public void Set(object value) => output[$"{typeof(T).Name}{(suffix != null ? $"_{suffix}" : string.Empty)}"] = value;
}
