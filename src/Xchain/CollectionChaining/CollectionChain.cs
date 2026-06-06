namespace Xchain;

/// <summary>
/// Base class for a centralized workflow topology declaration.
/// Subclass and override <see cref="Configure"/> to declare the collection execution order.
/// The source generator reads the <c>Configure</c> method body and emits all
/// <c>[CollectionDefinition]</c> classes and <c>[Collection]</c> attributes automatically.
/// </summary>
/// <remarks>
/// <code>
/// public class WorkflowA : WorkflowChain
/// {
///     protected override void Configure(IWorkflowBuilder b) => b
///         .Start&lt;Step_01_Client&gt;()
///         .Then&lt;Step_02_Project&gt;()
///         .End&lt;Step_03_Import&gt;();
/// }
/// </code>
/// Each test class referenced in <c>Configure</c> must be declared <c>partial</c>.
/// </remarks>
public abstract class WorkflowChain
{
    protected abstract void Configure(IWorkflowBuilder builder);
}
