using Xunit.Sdk;

namespace Xchain.Tests
{
    [TraitDiscoverer("Xchain.TestChainTraitDiscoverer", "Xchain")]
    [AttributeUsage(AttributeTargets.Method)]
    public class ChainTagAttribute(string? owner = null, string? category = null, string? color = null) : Attribute, ITraitAttribute
    {
        public string? Owner { get; set; } = owner;
        public string? Category { get; set; } = category;
        public string? Color { get; set; } = color; 
    }
}
