using Xunit.Abstractions;

namespace Xchain;

/// <summary>
/// A fixture that blocks the execution of a test collection until another specified collection completes.
/// </summary>
/// <typeparam name="TCollection">The type representing the collection to wait for.</typeparam>
/// <remarks>
/// This fixture is used when a test collection depends on the output or completion of another.
/// Waits up to a specified timeout and optionally logs diagnostics using <see cref="IMessageSink"/>.
/// </remarks>
public class CollectionChainLinkAwaitFixture<TCollection>
{
    /// <summary>
    /// Waits for the specified collection to complete, using a default timeout of 360 seconds.
    /// </summary>
    public CollectionChainLinkAwaitFixture() =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, TimeSpan.FromSeconds(360));

    /// <summary>
    /// Waits for the specified collection to complete with diagnostics enabled.
    /// </summary>
    /// <param name="messageSink">Used to log test framework messages.</param>
    public CollectionChainLinkAwaitFixture(IMessageSink messageSink) =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, TimeSpan.FromSeconds(360), messageSink);

    /// <summary>
    /// Waits for the specified collection to complete using a custom timeout.
    /// </summary>
    /// <param name="timeout">Maximum time to wait for the collection to finish.</param>
    public CollectionChainLinkAwaitFixture(TimeSpan timeout) : base() =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, timeout);

    /// <summary>
    /// Waits for the specified collection to complete with a custom timeout and diagnostics.
    /// </summary>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <param name="messageSink">Used for diagnostic output.</param>
    public CollectionChainLinkAwaitFixture(TimeSpan timeout, IMessageSink messageSink) : base() =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).Name, timeout, messageSink);
}
