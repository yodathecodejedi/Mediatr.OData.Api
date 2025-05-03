using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using System.Data;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions;

public static class TypeExtensions
{
    public static bool TryGetReferenceProperties(this Type? type, out Dictionary<string, PropertyCategory> referenceProperties)
    {
        referenceProperties = default!;
        if (type is null)
            return false;

        try
        {
            Dictionary<string, PropertyCategory> intermediateProperties = [];
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                property.TryGetPropertyCategory(out PropertyCategory propertyCategory);
                if (propertyCategory != default! && propertyCategory == PropertyCategory.Navigation || propertyCategory == PropertyCategory.Object)
                {
                    if (property.CustomAttributes.Any(ca =>
                        ca.AttributeType == typeof(ODataIgnoreAttribute) ||
                        ca.AttributeType == typeof(ODataETagAttribute)
                        ) || property.Name.Equals("Hash"))
                    {
                        continue;
                    }
                    intermediateProperties.Add(property.Name.ToLower(), propertyCategory);
                }
            }
            if (intermediateProperties.Count > 0)
            {
                referenceProperties = intermediateProperties;
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetKeyProperty(this Type? type, out PropertyInfo propertyInfo)
    {
        propertyInfo = default!;
        if (type is null)
            return false;

        //ODataKey Attribute
        propertyInfo = type.GetProperties()
            .SingleOrDefault(x => x.GetCustomAttribute<ODataKeyAttribute>() is not null) ?? default!;

        //Implicit Key Attribute
        propertyInfo ??= type.GetProperty("Key") ?? default!;

        //Implicit ObjectKey Attribute
        propertyInfo ??= type.GetProperty(type.Name + "Key") ?? default!;

        //Implicit Id Attribute
        propertyInfo ??= type.GetProperty("Id") ?? default!;

        //Implicit ObjectId Attribute
        propertyInfo ??= type.GetProperty(type.Name + "Id") ?? default!;

        var internalPropertyNames = type.GetProperties().Where(x =>
            x.GetCustomAttribute<ODataIgnoreAttribute>() is not null ||
            x.GetCustomAttribute<ODataETagAttribute>() is not null
            ).Select(p => p.Name).ToList();

        if (propertyInfo is not null && internalPropertyNames.Contains(propertyInfo.Name))
        {
            propertyInfo = default!;
            throw new MissingPrimaryKeyException($"The property {propertyInfo.Name} of the DomainObject {type.Name} is marked as ODataIgnore or ODataETag, but is used as a key property. Please remove the ODataIgnore or ODataETag attribute from the property or add the ODataKey attribute to the property.");
        }
        if (propertyInfo is null)
        {
            throw new MissingPrimaryKeyException($"The DomainObject {type.Name} does not have a key property. Please add the ODataKey attribute to the property that contains the key of the DomainObject.");
        }
        return (propertyInfo is not null);
    }

    public static bool TryGetGroupRoute(this Type? type, out string route)
    {
        route = string.Empty;
        if (type is null)
            return false;
        Type? parentType = type.DeclaringType;
        if (parentType != null)
        {
            var endpointGroupAttribute = parentType.GetCustomAttribute<EndpointGroupAttribute>() ?? default!;
            if (endpointGroupAttribute != default!)
            {
                route = endpointGroupAttribute.GetPropertyValue("Route")?.ToString() ?? string.Empty;

            }
        }
        return !string.IsNullOrEmpty(route);
    }

    public static bool TryGetProduces(this Type? domainObjectType, Type? navigationObjectType, bool keyInRoute, EndpointMethod httpMethod, out Produces produces)
    {
        produces = default!;
        if (domainObjectType is null) return false;

        if (!keyInRoute)
        {
            if (httpMethod == EndpointMethod.Delete) { produces = Produces.Value; return true; }
            if (httpMethod == EndpointMethod.Get) { produces = Produces.IEnumerable; return true; }
            produces = Produces.Object;
            return true;
        }
        if (navigationObjectType is null)
        {
            produces = httpMethod != EndpointMethod.Delete ? Produces.Object : Produces.Value;
            return true;
        }
        produces = httpMethod != EndpointMethod.Delete ? domainObjectType.GetNavigationProduces(navigationObjectType) : Produces.Value;
        return true;
    }

    public static Produces GetNavigationProduces(this Type? domainObjectType, Type? navigationObjectType)
    {
        if (domainObjectType is null || navigationObjectType is null) return Produces.Value;

        if (!domainObjectType.TryGetReferenceProperties(out var referenceProperties)) return Produces.Value;

        PropertyInfo[] domainObjectPropertyInfos = domainObjectType.GetProperties();
        foreach (var referenceProperty in referenceProperties)
        {
            PropertyInfo? propertyInfo = domainObjectPropertyInfos.FirstOrDefault(pi => pi.Name.ToLower().Equals(referenceProperty.Key.ToLower()));
            if (propertyInfo is null) continue;
            if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType == navigationObjectType)
            {
                return Produces.Object;
            }
            if (propertyInfo.PropertyType.IsArray && propertyInfo.PropertyType.GetElementType() == navigationObjectType)
            {
                return Produces.IEnumerable;
            }
            if (propertyInfo.PropertyType.IsGenericType)
            {
                var genericDefinition = propertyInfo.PropertyType.GetGenericTypeDefinition();
                if (typeof(IEnumerable<>).IsAssignableFrom(genericDefinition) ||
                          typeof(ICollection<>).IsAssignableFrom(genericDefinition) ||
                          typeof(IList<>).IsAssignableFrom(genericDefinition) ||
                          typeof(List<>).IsAssignableFrom(genericDefinition))
                {
                    if (propertyInfo.PropertyType.GetGenericArguments()[0] == navigationObjectType)
                    {
                        return Produces.IEnumerable;
                    }
                }
            }
        }
        return Produces.Value;
    }

    public static bool TryGetRouteSegment(this Type? domainObjectType, Type? navigationObjectType, out string routeSegment)
    {
        routeSegment = string.Empty;
        if (domainObjectType is null || navigationObjectType is null) return true;

        if (!domainObjectType.TryGetReferenceProperties(out var referenceProperties)) return true;
        PropertyInfo[] domainObjectPropertyInfos = domainObjectType.GetProperties();

        foreach (var referenceProperty in referenceProperties)
        {
            PropertyInfo? propertyInfo = domainObjectPropertyInfos.FirstOrDefault(pi => pi.Name.ToLower().Equals(referenceProperty.Key.ToLower()));
            if (propertyInfo is null) continue;
            if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType == navigationObjectType)
            {
                routeSegment = referenceProperty.Key;
                break;
            }
            if (propertyInfo.PropertyType.IsArray && propertyInfo.PropertyType.GetElementType() == navigationObjectType)
            {
                routeSegment = referenceProperty.Key;
                break;
            }
            if (propertyInfo.PropertyType.IsGenericType)
            {
                var genericDefinition = propertyInfo.PropertyType.GetGenericTypeDefinition();
                if (typeof(IEnumerable<>).IsAssignableFrom(genericDefinition) ||
                          typeof(ICollection<>).IsAssignableFrom(genericDefinition) ||
                          typeof(IList<>).IsAssignableFrom(genericDefinition) ||
                          typeof(List<>).IsAssignableFrom(genericDefinition))
                {
                    if (propertyInfo.PropertyType.GetGenericArguments()[0] == navigationObjectType)
                    {
                        routeSegment = referenceProperty.Key;
                    }
                }
            }
        }
        return true;
    }
}
