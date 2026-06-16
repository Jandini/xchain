namespace Xchain;

/// <summary>
/// A single-constructor wrapper around <see cref="CollectionChainAwaitFixture{T}"/> for use in
/// <see cref="ICollectionFixture{T}"/> declarations.
/// </summary>
/// <typeparam name="T">The collection type to wait for.</typeparam>
/// <remarks>
/// <see cref="CollectionChainAwaitFixture{T}"/> exposes multiple public constructors (for custom timeouts
/// and <c>IMessageSink</c> diagnostics). xUnit requires exactly one public constructor on any type
/// registered via <see cref="ICollectionFixture{T}"/>. This sealed wrapper provides that guarantee
/// by inheriting only the parameterless default, which waits indefinitely.
///
/// Use <see cref="CollectionChainAwaitFixture{T}"/> directly only when subclassing to supply a custom timeout:
/// <code>
/// internal class MyAwait : CollectionChainAwaitFixture&lt;ProducerCollection&gt;
/// {
///     public MyAwait() : base(TimeSpan.FromMinutes(10)) { }
/// }
/// </code>
/// </remarks>
public sealed class CollectionChainAwait<T> : CollectionChainAwaitFixture<T>;
