using System.Collections.Concurrent;

namespace Xchain;

/// <summary>
/// A thread-safe stack that stores exceptions encountered during chained test execution.
/// Used by <see cref="TestChainContextFixture"/> to track failures and support conditional test skipping via LinkUnless and SkipIf.
/// </summary>
/// <remarks>
/// Inherits from <see cref="ConcurrentStack{T}"/> to ensure safe concurrent access in async and parallel test contexts.
/// </remarks>
public class TestChainErrors : ConcurrentStack<Exception>
{
}
