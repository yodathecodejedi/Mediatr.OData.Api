using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Models;
using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using System.Reflection;

namespace Mediatr.OData.Api.Serializers;

public class ODataGenericTypeResourceSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;
        if (resourceContext.ResourceInstance is not DomainObject) return;

        IEnumerable<PropertyInfo> propertyInfos = GetGenericTypePropertyInfos(resourceContext);
        if (propertyInfos is null || !propertyInfos.Any())
        {
            return;
        }

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            if (propertyInfo is null) continue;
            object value = resourceContext.ResourceInstance.GetPropertyValue(propertyInfo.Name) ?? default!;
            if (value is not null)
            {
                ODataTypeAnnotation annotation = new(value.ToString());
                serializerResult.Resource.TypeAnnotation = annotation;
            }
            serializerResult.Remove(propertyInfo.Name);
        }
    }

    private static IEnumerable<PropertyInfo> GetGenericTypePropertyInfos(ResourceContext resourceContext)
    {
        return resourceContext.ResourceInstance.GetType().GetProperties().Where(p =>
            p.GetCustomAttribute<ODataTypeAttribute>() is not null);
    }
}
