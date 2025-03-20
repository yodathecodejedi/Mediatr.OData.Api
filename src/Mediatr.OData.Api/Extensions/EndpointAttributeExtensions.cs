using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;

namespace Mediatr.OData.Api.Extensions;

public static class EndpointAttributeExtensions
{
    public static bool TryGetRoute(this EndpointAttribute endpointAttribute, out string route)
    {
        route = string.Empty;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            route = endpointAttribute.Route;
            return true;
        }
        catch
        {
            return false;
        }

    }

    public static bool TryGetKeyInRoute(this EndpointAttribute endpointAttribute, out bool keyInRoute)
    {
        keyInRoute = false;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            keyInRoute = endpointAttribute.KeyInRoute;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetRouteSegment(this EndpointAttribute endpointAttribute, out string routeSegment)
    {
        routeSegment = string.Empty;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            routeSegment = endpointAttribute.RouteSegment;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetHttpMethod(this EndpointAttribute endpointAttribute, out EndpointMethod httpMethod)
    {
        httpMethod = EndpointMethod.Get;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            httpMethod = endpointAttribute.HttpMethod;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetBinding(this EndpointAttribute endpointAttribute, out EndpointBinding binding)
    {
        binding = EndpointBinding.CustomBinding;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            binding = endpointAttribute.Binding;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetRoutePrefix(this EndpointAttribute endpointAttribute, out string routePrefix)
    {
        routePrefix = string.Empty;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            routePrefix = endpointAttribute.RoutePrefix ?? string.Empty;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetDomainObjectType(this EndpointAttribute endpointAttribute, out Type domainObjectType)
    {
        domainObjectType = default!;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            if (endpointAttribute.GetPropertyValue("DomainObjectType") is not Type propertyValue)
            {
                return false;
            }
            domainObjectType = propertyValue as Type;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetKeyType(this EndpointAttribute endpointAttribute, out Type keyType)
    {
        keyType = default!;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            if (endpointAttribute.GetPropertyValue("KeyType") is not Type propertyValue)
            {
                return false;
            }
            keyType = propertyValue as Type;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetNavigationObjectType(this EndpointAttribute endpointAttribute, out Type navigationObjectType)
    {
        navigationObjectType = default!;
        ArgumentNullException.ThrowIfNull(endpointAttribute);
        try
        {
            if (endpointAttribute.GetPropertyValue("NavigationObjectType") is not Type propertyValue)
            {
                return false;
            }
            navigationObjectType = propertyValue as Type;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
