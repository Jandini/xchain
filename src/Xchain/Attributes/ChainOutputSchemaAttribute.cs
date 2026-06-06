namespace Xchain;

/// <summary>
/// Marks a class or interface as an output key schema. The source generator emits one typed extension method
/// on <see cref="TestChainOutput"/> for each public property, deriving the key name from
/// <c>nameof</c> so renaming a property is a compile-time error at every call site.
/// </summary>
/// <remarks>
/// <code>
/// [ChainOutputSchema]
/// public interface IMyOutputs
/// {
///     Guid   ClientId  { get; }
///     string ProjectId { get; }
/// }
/// // Generator emits: output.ClientId&lt;T&gt;(), output.ProjectId&lt;T&gt;()
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class ChainOutputSchemaAttribute : Attribute { }
