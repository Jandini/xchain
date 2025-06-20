namespace Xchain;

/// <summary>
/// Assigns a Link number to a test collection for execution order.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CollectionChainOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
