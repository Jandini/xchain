namespace Xchain;

/// <summary>
/// Provides typed access to a value in <see cref="TestChainOutput"/> using a key based on a context type <typeparamref name="T"/>
/// and an optional suffix. The retrieved or stored value is of type <typeparamref name="TOutput"/>.
/// </summary>
/// <typeparam name="T">The context type used to generate the output key.</typeparam>
/// <typeparam name="TOutput">The type of the value to retrieve or store.</typeparam>
public class TestOutput<T, TOutput>(TestChainOutput output, string suffix = null)
{
    /// <summary>
    /// Retrieves a value of type <typeparamref name="TOutput"/> from the output dictionary
    /// using a key composed of <typeparamref name="T"/>'s type name and an optional suffix.
    /// </summary>
    /// <returns>The value associated with the generated key.</returns>
    public TOutput Get() => output.Get<TOutput>($"{typeof(T).Name}{(suffix != null ? $"_{suffix}" : string.Empty)}");

    /// <summary>
    /// Stores the given value in the output dictionary using a key composed of
    /// <typeparamref name="T"/>'s type name and an optional suffix.
    /// </summary>
    /// <param name="value">The value to store, expected to be of type <typeparamref name="TOutput"/>.</param>
    public void Put(object value) => output[$"{typeof(T).Name}{(suffix != null ? $"_{suffix}" : string.Empty)}"] = value;

    /// <summary>
    /// Attempts to retrieve a value of type <typeparamref name="TOutput"/> from the output dictionary
    /// using a key composed of <typeparamref name="T"/>'s type name and an optional suffix.
    /// </summary>
    /// <param name="value">When this method returns, contains the value associated with the key,
    /// if found and of the correct type; otherwise, the default value for <typeparamref name="TOutput"/>.</param>
    /// <returns><c>true</c> if the value was found and is of type <typeparamref name="TOutput"/>; otherwise, <c>false</c>.</returns>
    public bool TryGet(out TOutput value) => output.TryGetValue($"{typeof(T).Name}{(suffix != null ? $"_{suffix}" : string.Empty)}", out var obj) && obj is TOutput v && (value = v) != null || (value = default) == null;

}
