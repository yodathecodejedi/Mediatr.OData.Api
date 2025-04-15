using Microsoft.AspNetCore.OData.Deltas;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions;

public static class GenericExtensions
{
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
