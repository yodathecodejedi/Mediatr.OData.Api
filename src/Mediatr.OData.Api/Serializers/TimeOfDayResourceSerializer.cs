using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Mediatr.OData.Api.Serializers;

public class TimeOfDayResourceSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        foreach (ODataPropertyInfo oDataPropertyInfo in serializerResult.Remaining)
        {
            if (selectExpandNode.PropertyIsOfEdmKind(oDataPropertyInfo, typeof(TimeOnly)))
            {
                //Build to remove Default DateTime Values (0001-01-01 etc)
                serializerResult.Resource.TryGetODataProperty(oDataPropertyInfo, out ODataProperty oDataProperty);
                TimeOnly defaultValue = default!;

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
                if (oDataProperty.Value is not TimeOnly)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }
                if (oDataProperty.Value.Equals(defaultValue))
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                TimeOnly originalValue = (TimeOnly)oDataProperty.Value;

                try
                {
                    Type declaredType = GetDeclaredType(resourceContext, oDataPropertyInfo);

                    if (declaredType == typeof(TimeOnly))
                    {
                        TimeOnly newValue = new(originalValue.Hour, originalValue.Minute, originalValue.Second);

                        ODataProperty property = new()
                        {
                            Name = oDataProperty.Name,
                            Value = new ODataUntypedValue
                            {
                                RawValue = System.Text.Json.JsonSerializer.Serialize(newValue.ToString("HH:mm:ss"))
                            }
                        };
                        serializerResult.Add(property);
                        continue;
                    }
                }
                catch { }

                serializerResult.Add(new ODataProperty
                {
                    Name = oDataProperty.Name,
                    Value = new ODataPrimitiveValue(
                            new TimeOnly(originalValue.Hour, originalValue.Minute, originalValue.Second))
                }
                );
            }
        }
    }
}
