using Xunit;
using Xunit.Abstractions;

namespace Xchain;

/// <summary>
/// Orders test collections by the Link value defined in [ChainLink].
/// Collections without a Link default to 0.
/// </summary>
public class ChainLinker : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        return testCollections
            .Select(tc =>
            {
                var attr = tc.CollectionDefinition?.GetCustomAttributes(typeof(ChainLinkAttribute).AssemblyQualifiedName!)
                    .FirstOrDefault();

                int link = attr?.GetNamedArgument<int>(nameof(ChainLinkAttribute.Link)) ?? 0;

                return new { Collection = tc, Link = link };
            })
            .OrderBy(x => x.Link)
            .Select(x => x.Collection);
    }
}
