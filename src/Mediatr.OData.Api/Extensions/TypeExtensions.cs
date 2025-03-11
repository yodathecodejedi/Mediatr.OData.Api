using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions;

public static class TypeExtensions
{
    public static bool TryGetKeyProperty(this Type? type, out PropertyInfo propertyInfo)
    {
        propertyInfo = default!;
        if (type is null)
            return false;

        propertyInfo = type.GetProperties()
            .SingleOrDefault(x => x.GetCustomAttribute<KeyAttribute>() is not null) ?? default!;

        propertyInfo ??= type.GetProperty("Id") ?? default!;

        propertyInfo ??= type.GetProperty(type.Name + "Id") ?? default!;

        propertyInfo ??= type.GetProperty("Key") ?? default!;

        propertyInfo ??= type.GetProperty(type.Name + "Key") ?? default!;

        return (propertyInfo is not null);
    }
}
