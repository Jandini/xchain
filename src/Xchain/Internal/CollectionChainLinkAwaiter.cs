using System.Collections.Concurrent;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain;

/// <summary>
/// Coordinates synchronization between test collections by allowing one collection to wait
/// until another has completed. Used to control execution order in parallel test environments.
/// </summary>
/// <remarks>
/// Each registered collection gets a <see cref="ManualResetEventSlim"/> that is unsignaled while
/// the collection runs and signaled when it completes. Waiting collections block on that event
/// instead of polling, so there is no artificial delay between completion and continuation.
/// </remarks>
internal static class CollectionChainLinkAwaiter
{
    private static readonly ConcurrentDictionary<string, ManualResetEventSlim> ActiveCollections = new();

    // Returns the shared event for a collection, creating it on first access so that
    // waiters and registrants always get the same instance regardless of arrival order.
    private static ManualResetEventSlim GetOrCreate(string name) =>
        ActiveCollections.GetOrAdd(name, _ => new ManualResetEventSlim(false));

    /// <summary>
    /// Marks a collection as active (registered and executing).
    /// Creates the shared completion event if not yet created.
    /// </summary>
    public static void Register(string name) => GetOrCreate(name);

    /// <summary>
    /// Signals that a collection has completed so any waiting collections may continue.
    /// </summary>
    public static void Unregister(string name) => GetOrCreate(name).Set();

    /// <summary>
    /// Blocks until the specified collection completes or the timeout is reached.
    /// Safe to call before <see cref="Register"/> — both share the same event instance.
    /// </summary>
    /// <param name="name">The name of the collection to wait for.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="messageSink">Optional diagnostic sink for test framework output.</param>
    /// <exception cref="TimeoutException">Thrown if the timeout expires before the collection signals completion.</exception>
    public static void WaitForCollection(string name, TimeSpan timeout, IMessageSink? messageSink = null)
    {
        messageSink?.OnMessage(new DiagnosticMessage($"Waiting for collection '{name}' (timeout: {timeout.TotalSeconds}s)"));

        if (!GetOrCreate(name).Wait(timeout))
            throw new TimeoutException($"Timed out waiting for collection '{name}' to complete.");
    }
}



