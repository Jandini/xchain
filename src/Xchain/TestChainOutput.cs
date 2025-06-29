using System.Collections.Concurrent;

namespace Xchain;

/// <summary>
/// A thread-safe dictionary for sharing output between chained tests.
/// Used internally by <see cref="TestChainContextFixture"/> and <see cref="CollectionChainContextFixture"/> 
/// to store test state, outputs, and exception data across test methods or collections.
/// </summary>
/// <remarks>
/// Extends <see cref="ConcurrentDictionary{TKey,TValue}"/> with string keys and object values.
/// Keys must be unique within the context (class or collection).
/// </remarks>
public class TestChainOutput : ConcurrentDictionary<string, object>
{
}
