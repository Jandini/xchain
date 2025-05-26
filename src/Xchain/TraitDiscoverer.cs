using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain;

public class TraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        if (traitAttribute is not IReflectionAttributeInfo reflectionAttribute)
            yield break;

        var instance = reflectionAttribute.Attribute;
        var type = instance.GetType();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanRead && property.Name != "TypeId")
            {
                var value = property.GetValue(instance);

                if (value != null)
                    yield return new KeyValuePair<string, string>(property.Name, value.ToString());
            }
        }
    }
}
