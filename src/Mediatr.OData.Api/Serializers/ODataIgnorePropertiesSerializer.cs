using Mediatr.OData.Api.Abstractions.Attributes;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Mediatr.OData.Api.Serializers;

public class ODataIgnorePropertiesSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        IEnumerable<PropertyInfo> propertyInfos = GetInternalPropertyInfos(resourceContext);

        if (propertyInfos is null) return;
        if (!propertyInfos.Any()) return;
        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            serializerResult.Remove(propertyInfo.Name);
        }
    }

    private static IEnumerable<PropertyInfo> GetInternalPropertyInfos(ResourceContext resourceContext)
    {
        return resourceContext.ResourceInstance.GetType().GetProperties().Where(p =>
            p.GetCustomAttribute<InternalAttribute>() is not null ||
            p.GetCustomAttribute<InternalKeyAttribute>() is not null ||
            p.GetCustomAttribute<JsonIgnoreAttribute>() is not null);
    }
}
