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
    /// <param name="timeout">Maximum time to wait for the previous step. <see langword="null"/> (default) waits indefinitely.</param>
    IWorkflowBuilder Then<T>(TimeSpan? timeout = null);

    /// <summary>Last collection in the chain. Awaits the previous step and signals itself,
    /// allowing downstream cross-flow collections to await this flow's completion.</summary>
    /// <param name="timeout">Maximum time to wait for the previous step. <see langword="null"/> (default) waits indefinitely.</param>
    IWorkflowBuilder End<T>(TimeSpan? timeout = null);

    /// <summary>
    /// Declares a workflow-scoped DI fixture to be added as <c>ICollectionFixture&lt;T&gt;</c>
    /// to every step's <c>[CollectionDefinition]</c> class. The generator also emits
    /// <c>ICollectionFixture&lt;WorkflowTeardownFixture&lt;T&gt;&gt;</c> on the last step so the
    /// hosted services are stopped after the final collection completes.
    /// </summary>
    /// <typeparam name="T">A <c>WorkflowServiceProviderFixture&lt;T&gt;</c> subclass unique to this workflow.</typeparam>
    IWorkflowBuilder WithWorkflowFixture<T>();
}
