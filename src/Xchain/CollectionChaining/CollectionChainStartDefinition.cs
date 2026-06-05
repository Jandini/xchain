using Xunit;

namespace Xchain;

/// <summary>
/// Abstract base class for a <c>[CollectionDefinition]</c> that marks the first collection in a chain.
/// Inheriting classes automatically receive <see cref="CollectionChainSignalFixture{T}"/> (signals
/// completion to downstream waiters) and <see cref="CollectionChainContextFixture"/> (shared output/errors).
/// </summary>
/// <typeparam name="T">The test class type paired with this definition (usually the class in the same file).</typeparam>
/// <remarks>
/// xUnit picks up <see cref="ICollectionFixture{T}"/> from base class interfaces via reflection, so concrete
/// subclasses inherit both fixtures without re-declaring them. This class is abstract so xUnit does not
/// treat it as a collection definition on its own.
///
/// <code>
/// [CollectionDefinition("MyCollection")]
/// public class MyCollectionDefinition : CollectionChainStartDefinition&lt;MyTestClass&gt;;
/// </code>
///
/// Pair with <see cref="CollectionChainNextDefinition{TAwait, T}"/> for middle collections and
/// <see cref="CollectionChainEndDefinition{TAwait}"/> for the last collection in the chain.
/// </remarks>
public abstract class CollectionChainStartDefinition<T> :
    ICollectionFixture<CollectionChainSignalFixture<T>>,
    ICollectionFixture<CollectionChainContextFixture>;
