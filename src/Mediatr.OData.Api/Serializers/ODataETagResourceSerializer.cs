using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using System.Reflection;

namespace Mediatr.OData.Api.Serializers;

public class ODataETagResourceSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        PropertyInfo propertyInfo = GetETagPropertyInfo(resourceContext);

        //Hier zouden we nog iets kunnen toevoegen dat we automatisch een ETag maken als er geen Hash veld bestaat
        //alle velden die in de ETag moeten komen worden dan geconcateneerd

        if (propertyInfo is null) return;

        object value = resourceContext.ResourceInstance.GetPropertyValue(propertyInfo.Name) ?? default!;
        if (value is not null)
        {
            var eTag = EncodeETag(value.ToString());
            if (eTag is not null) serializerResult.SetETag(eTag);
        }
        serializerResult.Remove(propertyInfo.Name);
    }

    private static PropertyInfo GetETagPropertyInfo(ResourceContext resourceContext)
    {
        return resourceContext.ResourceInstance.GetType().GetProperties().SingleOrDefault(
            p => p.Name.Equals("Hash") || p.Name.Equals("ETag") ||
            p.GetCustomAttribute<ODataETagAttribute>() is not null ||
            p.GetCustomAttribute<PropertyHashAttribute>() is not null ||
            p.GetCustomAttribute<PropertyModeAttribute>()?.Mode == Mode.ETag ||
            p.GetCustomAttribute<PropertyModeAttribute>()?.Mode == Mode.Hash) ?? default!;
    }

    private static string EncodeETag(string? hash)
    {
        if (hash is null) return default!;
        if (hash.Length == 0) return default!;

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(hash);
        return Convert.ToBase64String(bytes);
    }
}
