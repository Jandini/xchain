namespace Xchain.Tests.CollectionsSharedOutput;

public static class TestChainOutputExtensions
{
    public static TestOutput<T, Guid> SharedId<T>(this TestChainOutput output) =>
        new(output, "SharedId");
}
