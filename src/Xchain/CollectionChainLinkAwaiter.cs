using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain;

/// <summary>
/// Coordinates synchronization between test collections by allowing one collection to wait
/// until another has completed. Used to control execution order in parallel test environments.
/// </summary>
/// <remarks>
/// A collection is marked as "active" on registration and "inactive" on unregistration.
/// Waiting collections poll until the specified collection becomes inactive or the timeout expires.
/// 
/// TODO: Make this part of the registration, and add logging...
/// </remarks>
internal static class CollectionChainLinkAwaiter
{
    private static readonly ConcurrentDictionary<string, bool> ActiveCollections = new();

    /// <summary>
    /// Marks a collection as active (registered and executing).
    /// </summary>
    public static void Register(string name) => ActiveCollections[name] = true;

    /// <summary>
    /// Marks a collection as complete (no longer executing).
    /// </summary>
    public static void Unregister(string name) => ActiveCollections[name] = false;

    /// <summary>
    /// Waits for the specified collection to complete execution or until the timeout is reached.
    /// </summary>
    /// <param name="name">The name of the collection to wait for.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="messageSink">Optional diagnostic sink for test framework output.</param>
    /// <exception cref="TimeoutException">Thrown if the timeout expires before the collection is marked complete.</exception>
    public static void WaitForCollection(string name, TimeSpan timeout, IMessageSink messageSink = null)
    {
        var sw = Stopwatch.StartNew();

        // TODO: Consider integrating this with a more robust registration/logging system
        messageSink?.OnMessage(new DiagnosticMessage($"A collection is waiting for {name} up to {timeout.TotalSeconds} seconds"));

        while (true)
        {
            if (ActiveCollections.ContainsKey(name) && !ActiveCollections[name])
                break;

            if (sw.Elapsed > timeout)
                throw new TimeoutException($"Timed out waiting for collection '{name}' to complete.");

            Thread.Sleep(500);
        }
    }
}



