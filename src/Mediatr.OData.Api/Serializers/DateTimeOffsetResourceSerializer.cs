using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;

namespace Mediatr.OData.Api.Serializers;

public class DateTimeOffsetResourceSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        foreach (ODataPropertyInfo oDataPropertyInfo in serializerResult.Remaining)
        {
            if (selectExpandNode.PropertyIsOfEdmKind(oDataPropertyInfo, typeof(DateTimeOffset)))
            {
                //Build to remove Default DateTime Values (0001-01-01 etc)
                serializerResult.Resource.TryGetODataProperty(oDataPropertyInfo, out ODataProperty oDataProperty);
                DateTimeOffset defaultValue = default!;


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
                if (oDataProperty.Value is not DateTimeOffset)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }
                if (oDataProperty.Value.Equals(defaultValue))
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }
                DateTimeOffset originalValue = (DateTimeOffset)oDataProperty.Value;

                try
                {
                    Type declaredType = GetDeclaredType(resourceContext, oDataPropertyInfo);

                    //DateTime Serialization
                    if (declaredType == typeof(DateTime))
                    {
                        DateTime newValue = originalValue.LocalDateTime;
                        ODataProperty property = new()
                        {
                            Name = oDataProperty.Name,
                            Value = new ODataUntypedValue
                            {
                                RawValue = System.Text.Json.JsonSerializer.Serialize(newValue.ToString("yyyy-MM-ddTHH:mm:ss"))
                            }
                        };
                        serializerResult.Add(property);
                        continue;
                    }

                    if (declaredType == typeof(DateTimeOffset))
                    {
                        ODataProperty property = new()
                        {
                            Name = oDataProperty.Name,
                            Value = new ODataUntypedValue
                            {
                                RawValue = System.Text.Json.JsonSerializer.Serialize(originalValue.ToString("yyyy-MM-ddTHH:mm:sszzz"))
                            }
                        };
                        serializerResult.Add(property);
                        continue;
                    }

                    //We missed a valueType so we don't do anything
                }
                catch
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                }
            }
        }
    }
}
