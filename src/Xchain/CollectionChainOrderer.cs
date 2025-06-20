using Xunit;
using Xunit.Abstractions;

namespace Xchain;

/// <summary>
/// Orders test collections by the Link value defined in [ChainLink].
/// Collections without a Link default to int.MaxValue so they will be added at the end.
/// </summary>
public class CollectionChainOrderer : ITestCollectionOrderer
{
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
        {
            yield return collection;
        }

    }
}
