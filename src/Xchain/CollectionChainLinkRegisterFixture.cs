namespace Xchain;

/// <summary>
/// Registers a test collection with the internal awaiter so that dependent collections can wait on its completion.
/// </summary>
/// <typeparam name="T">A type used as the unique identifier for the collection (usually the test class).</typeparam>
/// <remarks>
/// Registration occurs during fixture construction and is removed on disposal.
/// This fixture should be attached to a collection that other collections depend on via <see cref="CollectionChainLinkAwaitFixture{T}"/>.
/// </remarks>
public class CollectionChainLinkRegisterFixture<T> : IDisposable
{
    /// <summary>
    /// Registers the collection using the name of the <typeparamref name="T"/> type.
    /// </summary>
    public CollectionChainLinkRegisterFixture() =>
        CollectionChainLinkAwaiter.Register(typeof(T).Name);

    /// <summary>
    /// Marks the registered collection as complete so that any waiting collections may continue.
    /// </summary>
    public void Dispose() =>
        CollectionChainLinkAwaiter.Unregister(typeof(T).Name);
}
