namespace Xchain;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LinkAttribute(int order) : Attribute
{
    public int Order { get; } = order;
    public string GetDisplayName() => $"Order = {Order}";
}
