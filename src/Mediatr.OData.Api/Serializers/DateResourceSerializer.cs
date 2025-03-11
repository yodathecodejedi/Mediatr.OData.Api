using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
namespace Mediatr.OData.Api.Serializers;


public class DateResourceSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        foreach (ODataPropertyInfo oDataPropertyInfo in serializerResult.Remaining)
        {
            if (selectExpandNode.PropertyIsOfEdmKind(oDataPropertyInfo, typeof(Date)))
            {
                //Build to remove Default DateTime Values (0001-01-01 etc)
                serializerResult.Resource.TryGetODataProperty(oDataPropertyInfo, out ODataProperty oDataProperty);
                Date defaultValue = default!;
                //If we don't get a oDataProperty we can't render anything  
                if (oDataProperty is null)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }
                if (oDataProperty.Value is null)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }
                if (oDataProperty.Value is not Date)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }
                if (oDataProperty.Value.Equals(defaultValue))
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }
                serializerResult.Add(oDataPropertyInfo);
            }
        }
    }
}
