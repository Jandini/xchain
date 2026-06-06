using Xunit.Sdk;

namespace Xchain.Tests;

[TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MetadataAttribute(string category) : Attribute, ITraitAttribute
{
    public string Category { get; } = category;
}
