using Xunit;
using Xunit.Abstractions;

namespace Xchain;

/// <summary>
/// Orders test collections by the Link value defined in [ChainLink].
/// Collections without a Link default to int.MaxValue so they will be added at the end.
/// </summary>
public class ChainLinker : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        var ordered = testCollections
            .Select(tc =>
            {
                var attr = tc.CollectionDefinition?.GetCustomAttributes(typeof(ChainLinkAttribute).AssemblyQualifiedName!)
                    .FirstOrDefault();

                int link = attr?.GetNamedArgument<int>(nameof(ChainLinkAttribute.Link)) ?? int.MaxValue;

                return new { Collection = tc, Link = link };
            })
            .OrderBy(x => x.Link)
            .Select(x => x.Collection);

        foreach (var collection in ordered)
        {
            yield return collection;
        }

    }
}
