using Xunit;
using Xunit.Abstractions;

namespace Xchain;

/// <summary>
/// Orders test collections based on the <see cref="CollectionChainOrderAttribute"/> applied to each collection definition.
/// </summary>
/// <remarks>
/// This orderer only takes effect if test parallelism is disabled across the assembly:
/// <code>[assembly: CollectionBehavior(DisableTestParallelization = true)]</code>
/// In most scenarios, prefer <see cref="CollectionChainLinkAwaitFixture{T}"/> for flexible, parallel-safe coordination.
/// </remarks>
public class CollectionChainOrderer : ITestCollectionOrderer
{
    /// <summary>
    /// Sorts collections by their assigned order value or defaults to <see cref="int.MaxValue"/> if not specified.
    /// </summary>
    /// <param name="testCollections">All test collections defined in the test assembly.</param>
    /// <returns>The ordered sequence of test collections.</returns>
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        var ordered = testCollections
            .Select(tc =>
            {
                var attr = tc.CollectionDefinition?.GetCustomAttributes(typeof(CollectionChainOrderAttribute).AssemblyQualifiedName!)
                    .FirstOrDefault();

                int order = attr?.GetNamedArgument<int>(nameof(CollectionChainOrderAttribute.Order)) ?? int.MaxValue;

                return new { Collection = tc, Order = order };
            })
            .OrderBy(x => x.Order)
            .Select(x => x.Collection);

        foreach (var collection in ordered)
            yield return collection;
    }
}
