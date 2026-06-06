namespace Xchain;

/// <summary>
/// Fluent API for declaring a workflow topology inside <see cref="WorkflowChain.Configure"/>.
/// The source generator reads these method calls to emit collection fixtures — none of these methods
/// are called at runtime.
/// </summary>
public interface IWorkflowBuilder
{
    /// <summary>Adds a cross-flow upstream dependency that the next step will await.</summary>
    IWorkflowBuilder After<T1>();

    /// <summary>Adds two cross-flow upstream dependencies that the next step will await.</summary>
    IWorkflowBuilder After<T1, T2>();

    /// <summary>First collection in the chain. Signals itself on completion.</summary>
    IWorkflowBuilder Start<T>();

    /// <summary>Middle collection. Awaits the previous step and signals itself.</summary>
    IWorkflowBuilder Then<T>();

    /// <summary>Last collection in the chain. Awaits the previous step and signals itself,
    /// allowing downstream cross-flow collections to await this flow's completion.</summary>
    IWorkflowBuilder End<T>();
}
