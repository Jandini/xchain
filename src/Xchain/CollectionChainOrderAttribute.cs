namespace Xchain;

/// <summary>
/// Assigns an explicit execution order to test collections when using the <see cref="CollectionChainOrderer"/>.
/// </summary>
/// <remarks>
/// This is only effective when parallel execution is disabled at the assembly level.
/// Most real-world scenarios will benefit more from <see cref="CollectionChainLinkAwaitFixture{T}"/>,
/// which supports coordination even with parallelism enabled.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CollectionChainOrderAttribute(int order) : Attribute
{
    /// <summary>
    /// Gets the collection's execution order relative to others in the assembly.
    /// Lower values run earlier.
    /// </summary>
    public int Order { get; } = order;
}
