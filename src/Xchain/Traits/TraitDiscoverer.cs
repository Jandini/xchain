using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain;

/// <summary>
/// A custom trait discoverer that converts the public properties of an attribute class into Xunit traits.
/// Enables creation of strongly-typed metadata attributes that annotate tests with rich, structured traits.
/// </summary>
/// <example>
/// Example usage:
/// <code>
/// [TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]
/// [AttributeUsage(AttributeTargets.Method)]
/// public class ChainTagAttribute(string? owner = null, string? category = null, string? color = null)
///     : Attribute, ITraitAttribute
/// {
///     public string? Owner { get; set; } = owner;
///     public string? Category { get; set; } = category;
///     public string? Color { get; set; } = color;
/// }
/// </code>
/// This will produce traits like:
/// - Owner: Dev
/// - Category: Regression
/// - Color: Red
/// </example>
public class TraitDiscoverer : ITraitDiscoverer
{
    /// <summary>
    /// Extracts traits from the given attribute by reflecting on its public properties.
    /// </summary>
    /// <param name="traitAttribute">The attribute to inspect for trait values.</param>
    /// <returns>A sequence of key-value trait pairs based on readable properties.</returns>
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        if (traitAttribute is not IReflectionAttributeInfo reflectionAttribute)
            yield break;

        var instance = reflectionAttribute.Attribute;
        var type = instance.GetType();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Exclude internal Xunit metadata
            if (property.CanRead && property.Name != "TypeId")
            {
                var value = property.GetValue(instance);

                if (value != null)
                    yield return new KeyValuePair<string, string>(property.Name, value.ToString());
            }
        }
    }
}
