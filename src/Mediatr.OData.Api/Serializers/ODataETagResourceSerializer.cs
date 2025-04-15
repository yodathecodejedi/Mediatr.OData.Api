using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Models;
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

        IEnumerable<PropertyInfo> propertyInfos = GetETagPropertyInfos(resourceContext);
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
                //Convert the value to the actual ByteArray
                byte[] byteArray = value as byte[] ?? default!;
                var eTag = EncodeETag(byteArray);
                if (eTag is not null) serializerResult.SetETag(eTag);
            }
            serializerResult.Remove(propertyInfo.Name);
        }
    }

    private static IEnumerable<PropertyInfo> GetETagPropertyInfos(ResourceContext resourceContext)
    {
        return resourceContext.ResourceInstance.GetType().GetProperties().Where(
            //Based on the implicit name of the property ETag or Hash
            p => p.Name.Equals("Hash") || p.Name.Equals("ETag") ||
            //Based on the attributes
            p.GetCustomAttribute<ODataETagAttribute>() is not null);
    }

    private static string EncodeETag(byte[] byteArray)
    {
        if (byteArray is null) return default!;
        if (byteArray.Length == 0) return default!;

        //Check the config for the output style
        var oDataConfiguration = AppContext.GetData("ODataConfiguration") as ODataConfiguration;
        var encodeBase64 = oDataConfiguration?.Formatting.EncodeETagBase64 ?? false;
        if (encodeBase64)
        {
            return Convert.ToBase64String(byteArray);
        }
        return $"0x{BitConverter.ToString(byteArray).Replace("-", "")}";
    }
}
