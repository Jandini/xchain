using Xunit;

namespace Xchain;

/// <summary>
/// Abstract base class for a <c>[CollectionDefinition]</c> that is the last collection in a chain.
/// Inheriting classes automatically await <typeparamref name="TAwait"/> before starting and do not
/// signal any downstream collection.
/// </summary>
/// <typeparam name="TAwait">The test class type whose collection must complete before this one starts.</typeparam>
/// <remarks>
/// Wraps <see cref="CollectionChainAwait{T}"/> and <see cref="CollectionChainContextFixture"/>
/// in a single inheritable base. Completes the Start / Next / End trilogy so every chain position
/// has a corresponding one-line definition class.
///
/// <code>
/// [CollectionDefinition("MyCollection")]
/// public class MyCollectionDefinition : CollectionChainEndDefinition&lt;UpstreamClass&gt;;
/// </code>
///
/// Pair with <see cref="CollectionChainStartDefinition{T}"/> for the first collection and
/// <see cref="CollectionChainNextDefinition{TAwait, T}"/> for middle collections.
/// </remarks>
public abstract class CollectionChainEndDefinition<TAwait> :
    ICollectionFixture<CollectionChainAwait<TAwait>>,
    ICollectionFixture<CollectionChainContextFixture>;
