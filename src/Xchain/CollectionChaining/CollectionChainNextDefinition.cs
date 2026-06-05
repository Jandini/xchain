using Xunit;

namespace Xchain;

/// <summary>
/// Abstract base class for a <c>[CollectionDefinition]</c> that sits in the middle of a chain.
/// Inheriting classes automatically await <typeparamref name="TAwait"/> before starting and signal
/// <typeparamref name="T"/> on completion so downstream collections can proceed.
/// </summary>
/// <typeparam name="TAwait">The test class type whose collection must complete before this one starts.</typeparam>
/// <typeparam name="T">The test class type paired with this definition (usually the class in the same file).</typeparam>
/// <remarks>
/// Wraps <see cref="CollectionChainNextFixture{TAwait, TRegister}"/> and <see cref="CollectionChainContextFixture"/>
/// in a single inheritable base so the collection definition requires only one line.
///
/// <code>
/// [CollectionDefinition("MyCollection")]
/// public class MyCollectionDefinition : CollectionChainNextDefinition&lt;UpstreamClass, MyTestClass&gt;;
/// </code>
///
/// Pair with <see cref="CollectionChainStartDefinition{T}"/> for the first collection and
/// <see cref="CollectionChainEndDefinition{TAwait}"/> for the last.
/// </remarks>
public abstract class CollectionChainNextDefinition<TAwait, T> :
    ICollectionFixture<CollectionChainNextFixture<TAwait, T>>,
    ICollectionFixture<CollectionChainContextFixture>;
