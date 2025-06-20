using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain;

// TODO: Make this part of the registration, and add logging...

internal static class CollectionChainLinkAwaiter
{
    private static readonly ConcurrentDictionary<string, bool> ActiveCollections = new();
    public static void Register(string name) => ActiveCollections[name] = true;
    public static void Unregister(string name) => ActiveCollections[name] = false;
    public static void WaitForCollection(string name, TimeSpan timeout, IMessageSink messageSink = null)
    {
        var sw = Stopwatch.StartNew();

        messageSink?.OnMessage(new DiagnosticMessage($"A collection is waiting for {name} up to {timeout.TotalSeconds} seconds"));

        while (true)
        {
            if (ActiveCollections.ContainsKey(name) && !ActiveCollections[name])
                break;

            if (sw.Elapsed.TotalSeconds > timeout.TotalSeconds)
                throw new TimeoutException($"Timed out waiting for collection '{name}' to complete.");

            Thread.Sleep(500);
        }
    }
}
