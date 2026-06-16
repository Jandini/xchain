using Xunit.Abstractions;

namespace Xchain;

/// <summary>
/// A fixture that blocks the execution of a test collection until another specified collection completes.
/// </summary>
/// <typeparam name="TCollection">The type representing the collection to wait for.</typeparam>
/// <remarks>
/// This fixture is used when a test collection depends on the output or completion of another.
/// By default the wait is infinite; subclass and call <c>base(TimeSpan)</c> to impose a limit.
/// Optionally logs diagnostics using <see cref="IMessageSink"/>.
/// </remarks>
public class CollectionChainAwaitFixture<TCollection>
{
    /// <summary>
    /// Waits indefinitely for the specified collection to complete.
    /// </summary>
    public CollectionChainAwaitFixture() =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).FullName ?? typeof(TCollection).Name);

    /// <summary>
    /// Waits indefinitely for the specified collection to complete, with diagnostics enabled.
    /// </summary>
    /// <param name="messageSink">Used to log test framework messages.</param>
    public CollectionChainAwaitFixture(IMessageSink messageSink) =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).FullName ?? typeof(TCollection).Name, messageSink: messageSink);

    /// <summary>
    /// Waits for the specified collection to complete using a custom timeout.
    /// </summary>
    /// <param name="timeout">Maximum time to wait for the collection to finish.</param>
    protected CollectionChainAwaitFixture(TimeSpan timeout) =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).FullName ?? typeof(TCollection).Name, timeout);

    /// <summary>
    /// Waits for the specified collection to complete with a custom timeout and diagnostics.
    /// </summary>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <param name="messageSink">Used for diagnostic output.</param>
    protected CollectionChainAwaitFixture(TimeSpan timeout, IMessageSink messageSink) =>
        CollectionChainLinkAwaiter.WaitForCollection(typeof(TCollection).FullName ?? typeof(TCollection).Name, timeout, messageSink);
}
