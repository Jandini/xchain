
namespace Xchain;

/// <summary>
/// Provides typed access to a value in <see cref="TestChainOutput"/> using a key based on a context type <typeparamref name="TCollection"/>
/// and an optional suffix. The retrieved or stored value is of type <typeparamref name="TOutput"/>.
/// </summary>
/// <typeparam name="TCollection">The context type used to generate the output key.</typeparam>
/// <typeparam name="TOutput">The type of the value to retrieve or store.</typeparam>
public class TestOutput<TCollection, TOutput>(TestChainOutput output, string suffix = null)
{
    public string Key { get => $"{typeof(TCollection).Name}{(suffix != null ? $"_{suffix}" : string.Empty)}"; }

    /// <summary>
    /// Retrieves a value of type <typeparamref name="TOutput"/> from the output dictionary
    /// using a key composed of <typeparamref name="TCollection"/>'s type name and an optional suffix.
    /// </summary>
    /// <returns>The value associated with the generated key.</returns>
    public TOutput Get() => output.Get<TOutput>(Key);

    /// <summary>
    /// Stores the given value in the output dictionary using a key composed of
    /// <typeparamref name="TCollection"/>'s type name and an optional suffix.
    /// </summary>
    /// <param name="value">The value to store, expected to be of type <typeparamref name="TOutput"/>.</param>
    public void Put(object value) => output[Key] = value;

    /// <summary>
    /// Attempts to retrieve a value of type <typeparamref name="TOutput"/> from the output dictionary
    /// using a key composed of <typeparamref name="TCollection"/>'s type name and an optional suffix.
    /// </summary>
    /// <param name="value">When this method returns, contains the value associated with the key,
    /// if found and of the correct type; otherwise, the default value for <typeparamref name="TOutput"/>.</param>
    /// <returns><c>true</c> if the value was found and is of type <typeparamref name="TOutput"/>; otherwise, <c>false</c>.</returns>
    public bool TryGet(out TOutput value) => output.TryGetValue(Key, out var obj) && obj is TOutput v && (value = v) != null || (value = default) == null;

    public bool ContainsKey() => output.ContainsKey(Key);
}
