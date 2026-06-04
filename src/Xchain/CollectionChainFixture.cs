namespace Xchain;

/// <summary>
/// A combined fixture that waits for <typeparamref name="TAwait"/> to complete and then registers
/// <typeparamref name="TRegister"/> so that downstream collections can wait for it in turn.
/// </summary>
/// <typeparam name="TAwait">The collection type to wait for before running.</typeparam>
/// <typeparam name="TRegister">The collection type being registered (typically the owning test class).</typeparam>
/// <remarks>
/// Replaces the two-fixture pattern of separately deriving from <see cref="CollectionChainLinkAwaitFixture{T}"/>
/// and <see cref="CollectionChainLinkSetupFixture{T}"/>. Use this when a collection both depends on another
/// and is itself a dependency for downstream collections.
///
/// <code>
/// [CollectionDefinition("SecondCollection")]
/// public class SecondCollectionDefinition :
///     ICollectionFixture&lt;CollectionChainFixture&lt;ProducerCollection, ConsumerCollection&gt;&gt;,
///     ICollectionFixture&lt;CollectionChainContextFixture&gt;;
/// </code>
/// </remarks>
public class CollectionChainFixture<TAwait, TRegister> : IDisposable
{
    /// <summary>
    /// Waits for <typeparamref name="TAwait"/> to complete (default 360-second timeout)
    /// and registers <typeparamref name="TRegister"/> for downstream consumers.
    /// </summary>
    public CollectionChainFixture() : this(TimeSpan.FromSeconds(360)) { }

    /// <summary>
    /// Waits for <typeparamref name="TAwait"/> to complete with a custom timeout
    /// and registers <typeparamref name="TRegister"/> for downstream consumers.
    /// Subclass with a public parameterless constructor to use a custom timeout.
    /// </summary>
    protected CollectionChainFixture(TimeSpan timeout)
    {
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TAwait).Name, timeout);
        CollectionChainLinkAwaiter.Register(typeof(TRegister).Name);
    }

    /// <summary>
    /// Signals that <typeparamref name="TRegister"/> has completed.
    /// </summary>
    public void Dispose() =>
        CollectionChainLinkAwaiter.Unregister(typeof(TRegister).Name);
}
