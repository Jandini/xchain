using Xunit.Sdk;

namespace Xchain;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class LinkAttribute(int order) : Attribute
{
    public int Order { get; } = order;
    public string GetDisplayName() => $"Order = {Order}";
}
