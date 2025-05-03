using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Microsoft.OData.Edm;
using System.Collections;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions;

public static class PropertyInfoExtensions
{
    #region PropertyCategory
    public static bool TryGetPropertyCategory(this PropertyInfo propertyInfo, out PropertyCategory propertyCategory)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        propertyCategory = PropertyCategory.Unknown;
        try
        {
            var type = propertyInfo.PropertyType;
            if (type.GetCustomAttributes<ODataKeyAttribute>().Any()) { propertyCategory = PropertyCategory.Key; return true; }
            if (type.IsArray) { propertyCategory = PropertyCategory.Navigation; return true; }

            if (type.IsEnum) { propertyCategory = PropertyCategory.Value; return true; }

            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(List<>) || genericType == typeof(IList<>)) { propertyCategory = PropertyCategory.Navigation; return true; }

                if (genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>)) { propertyCategory = PropertyCategory.Navigation; return true; }
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string)) { propertyCategory = PropertyCategory.Navigation; return true; }

            if (type.IsClass && type.Namespace != "System") { propertyCategory = PropertyCategory.Object; return true; }    // Exclude system types

            if (type.Namespace == "System") { propertyCategory = PropertyCategory.Value; return true; }    // Exclude system types

            return false;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region IsTypeNullable

    public static bool TryGetNullablePropertyInfo(this PropertyInfo propertyInfo, out object nullablePropertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));
        nullablePropertyInfo = default!;
        if (!propertyInfo.PropertyType.IsValueType) { nullablePropertyInfo = propertyInfo; return true; }

        var nullableAttr = propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "NullableAttribute");
        if (nullableAttr != null && nullableAttr.ConstructorArguments.Count > 0)
        {
            var flag = (byte)nullableAttr.ConstructorArguments[0].Value!;
            if (flag == 2) { nullablePropertyInfo = propertyInfo; return true; } // 2 means explicitly nullable
            return false;
        }

        if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null) { nullablePropertyInfo = propertyInfo; return true; }
        return false;
    }

    public static bool IsTypeNullable(this PropertyInfo property)
    {
        if (!property.PropertyType.IsValueType)
            return true; // Reference types are nullable unless annotated as non-nullable

        var nullableAttr = property.CustomAttributes
            .FirstOrDefault(a => a.AttributeType.Name == "NullableAttribute");

        if (nullableAttr != null && nullableAttr.ConstructorArguments.Count > 0)
        {
            var flag = (byte)nullableAttr.ConstructorArguments[0].Value!;
            return flag == 2; // 2 means explicitly nullable
        }

        return Nullable.GetUnderlyingType(property.PropertyType) != null;
    }
    #endregion

    #region EdmMultiplicity type
    public static bool TryGetEdmMultiplicityType(this PropertyInfo propertyInfo, out EdmMultiplicity edmMultiplicity)
    {
        edmMultiplicity = EdmMultiplicity.Unknown;
        if (propertyInfo == default!) return false;
        if (!propertyInfo.TryGetPropertyCategory(out var propertyCategory)) return false;
        if (propertyCategory == PropertyCategory.Navigation)
        {
            edmMultiplicity = EdmMultiplicity.Many;
            return true;
        }
        if (propertyCategory == PropertyCategory.Object)
        {
            edmMultiplicity = propertyInfo.IsTypeNullable() ? EdmMultiplicity.ZeroOrOne : EdmMultiplicity.One;
            return true;
        }

        return false;
    }

    public static bool TryGetProduces(this PropertyInfo propertyInfo, out Produces produces)
    {
        produces = Produces.Value;
        if (propertyInfo == default!) return false;
        if (!propertyInfo.TryGetPropertyCategory(out var propertyCategory)) return false;
        if (propertyCategory == PropertyCategory.Navigation)
        {
            produces = Produces.IEnumerable;
            return true;
        }
        if (propertyCategory == PropertyCategory.Object)
        {
            produces = Produces.Object;
            return true;
        }

        return false;
    }
    #endregion
}
