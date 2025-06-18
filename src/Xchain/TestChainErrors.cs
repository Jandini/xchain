using System.Collections.Concurrent;

namespace Xchain;

public class TestChainErrors : ConcurrentStack<Exception>
{
}
