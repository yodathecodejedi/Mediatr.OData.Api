using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using System.Reflection;

namespace Mediatr.OData.Api.Serializers;
public abstract class ResourceSerializer
{
    public abstract void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext);

    public ODataProperty GetODataProperty(ODataPropertyInfo propertyInfo, ODataResource oDataResource)
    {
        if (oDataResource.TryGetODataProperty(propertyInfo, out ODataProperty oDataProperty))
        {
            return oDataProperty;
        }
        return default!;
    }

    public Type GetDeclaredType(ResourceContext resourceContext, ODataPropertyInfo oDataPropertyInfo)
    {
        Type declaredEntityType = resourceContext.ResourceInstance.GetType();
        PropertyInfo declaredPropertyInfo = declaredEntityType.GetProperty(oDataPropertyInfo.Name) ?? default!;
        return Nullable.GetUnderlyingType(declaredPropertyInfo.PropertyType) ?? declaredPropertyInfo.PropertyType;
    }
}
