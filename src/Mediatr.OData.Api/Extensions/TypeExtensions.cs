using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions;

public static class TypeExtensions
{
    public static bool TryGetReferenceProperties(this Type? type, out Dictionary<string, PropertyCategory> navigationProperties)
    {
        navigationProperties = default!;
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
                    intermediateProperties.Add(property.Name.ToLower(), propertyCategory);
                }
            }
            if (intermediateProperties.Count > 0)
            {
                navigationProperties = intermediateProperties;
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

        //DataAnnotations Key attribute
        propertyInfo = type.GetProperties()
            .SingleOrDefault(x => x.GetCustomAttribute<KeyAttribute>() is not null) ?? default!;
        //ODataKey Attribute
        propertyInfo ??= type.GetProperties()
            .SingleOrDefault(x => x.GetCustomAttribute<ODataKeyAttribute>() is not null) ?? default!;

        //Mediatr OData Public Key Attribute
        propertyInfo ??= type.GetProperties()
            .SingleOrDefault(x => x.GetCustomAttribute<PublicKeyAttribute>() is not null) ?? default!;

        //Implicit Key Attribute
        propertyInfo ??= type.GetProperty("Key") ?? default!;

        //Implicit ObjectKey Attribute
        propertyInfo ??= type.GetProperty(type.Name + "Key") ?? default!;

        //Implicit Id Attribute
        propertyInfo ??= type.GetProperty("Id") ?? default!;

        //Implicit ObjectId Attribute
        propertyInfo ??= type.GetProperty(type.Name + "Id") ?? default!;

        var internalPropertyNames = type.GetProperties().Where(x =>
            x.GetCustomAttribute<InternalKeyAttribute>() is not null ||
            x.GetCustomAttribute<InternalAttribute>() is not null
            ).Select(p => p.Name).ToList();

        if (propertyInfo is not null && internalPropertyNames.Contains(propertyInfo.Name))
        {
            propertyInfo = default!;
            //Should we throw an exception here so the developer knows that something is wrong in usage ?
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
}
