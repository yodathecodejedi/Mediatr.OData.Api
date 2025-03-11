using Mediatr.OData.Api.Interfaces;
using Microsoft.AspNetCore.OData.Deltas;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions;

public static class GenericExtensions
{
    public static bool HasKeyProperty<T>(this T domainObject) where T : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(domainObject, nameof(domainObject));

        try
        {
            Type domainObjectType = domainObject.GetType();

            if (domainObject is Delta)
                domainObjectType = domainObject.GetType().GetGenericArguments()[0];

            var properties = domainObjectType.GetProperties();
            var domainObjectName = domainObjectType.Name;

            if (properties.Any(x => x.GetCustomAttribute<KeyAttribute>() is not null)) return true;

            if (properties.Any(x => x.Name.Equals("Id"))) return true;

            if (properties.Any(x => x.Name.Equals($"{domainObjectName}Id"))) return true;

            if (properties.Any(x => x.Name.Equals("Key"))) return true;

            if (properties.Any(x => x.Name.Equals($"{domainObjectName}Key"))) return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static PropertyInfo GetKeyProperty<T>(this T domainObject) where T : class
    {
        ArgumentNullException.ThrowIfNull(domainObject, nameof(domainObject));

        Type domainObjectType = domainObject.GetType();

        if (domainObject is Delta)
            domainObjectType = domainObject.GetType().GetGenericArguments()[0];

        domainObjectType.TryGetKeyProperty(out PropertyInfo keyPropertyInfo);

        return keyPropertyInfo ?? default!;
    }
}
