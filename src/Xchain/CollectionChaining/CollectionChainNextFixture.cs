namespace Xchain;

/// <summary>
/// A combined fixture that waits for <typeparamref name="TAwait"/> to complete and then registers
/// <typeparamref name="TRegister"/> so that downstream collections can wait for it in turn.
/// </summary>
/// <typeparam name="TAwait">The collection type to wait for before running.</typeparam>
/// <typeparam name="TRegister">The collection type being registered (typically the owning test class).</typeparam>
/// <remarks>
/// Replaces the two-fixture pattern of separately deriving from <see cref="CollectionChainAwaitFixture{T}"/>
/// and <see cref="CollectionChainSignalFixture{T}"/>. Use this when a collection both depends on another
/// and is itself a dependency for downstream collections.
///
/// <code>
/// [CollectionDefinition("SecondCollection")]
/// public class SecondCollectionDefinition :
///     ICollectionFixture&lt;CollectionChainNextFixture&lt;ProducerCollection, ConsumerCollection&gt;&gt;,
///     ICollectionFixture&lt;CollectionChainContextFixture&gt;;
/// </code>
/// </remarks>
public class CollectionChainNextFixture<TAwait, TRegister> : IDisposable
{
    /// <summary>
    /// Waits indefinitely for <typeparamref name="TAwait"/> to complete and registers
    /// <typeparamref name="TRegister"/> for downstream consumers.
    /// </summary>
    public CollectionChainNextFixture() : this((TimeSpan?)null) { }

    /// <summary>
    /// Waits for <typeparamref name="TAwait"/> to complete with a custom timeout
    /// and registers <typeparamref name="TRegister"/> for downstream consumers.
    /// Subclass with a public parameterless constructor to use a custom timeout.
    /// </summary>
    protected CollectionChainNextFixture(TimeSpan timeout) : this((TimeSpan?)timeout) { }

    /// <summary>
    /// Core constructor. Pass <see langword="null"/> for an infinite wait.
    /// </summary>
    protected CollectionChainNextFixture(TimeSpan? timeout)
    {
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TAwait).FullName ?? typeof(TAwait).Name, timeout);
        CollectionChainLinkAwaiter.Register(typeof(TRegister).FullName ?? typeof(TRegister).Name);
    }

    /// <summary>
    /// Signals that <typeparamref name="TRegister"/> has completed.
    /// </summary>
    public void Dispose() =>
        CollectionChainLinkAwaiter.Unregister(typeof(TRegister).FullName ?? typeof(TRegister).Name);
}
