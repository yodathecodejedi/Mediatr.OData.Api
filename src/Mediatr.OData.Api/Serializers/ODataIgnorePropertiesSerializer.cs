using Mediatr.OData.Api.Abstractions.Attributes;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using System.Reflection;

namespace Mediatr.OData.Api.Serializers;

public class ODataIgnorePropertiesSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        IEnumerable<PropertyInfo> propertyInfos = GetODataIgnoreProperties(resourceContext);

        if (propertyInfos is null) return;
        if (!propertyInfos.Any()) return;
        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            serializerResult.Remove(propertyInfo.Name);
        }
    }

    private static IEnumerable<PropertyInfo> GetODataIgnoreProperties(ResourceContext resourceContext)
    {
        return resourceContext.ResourceInstance.GetType().GetProperties().Where(p =>
            p.GetCustomAttribute<ODataIgnoreAttribute>() is not null);
    }
}
