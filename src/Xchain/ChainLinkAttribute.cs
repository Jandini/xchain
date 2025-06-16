namespace Xchain;

/// <summary>
/// Assigns a Link number to a test collection for execution order.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ChainLinkAttribute(int link) : Attribute
{
    public int Link { get; } = link;
}
