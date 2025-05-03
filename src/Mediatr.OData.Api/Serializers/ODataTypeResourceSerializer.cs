using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;

namespace Mediatr.OData.Api.Serializers;
public class ODataTypeResourceSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        //Hier aan het genericDomainObject type of zijn kinderen overslaan.
        if (resourceContext.StructuredType.TypeKind is Microsoft.OData.Edm.EdmTypeKind.Entity)
        {
            if (resourceContext.TryGetODataTypeName(out string oDataTypeName))
            {
                ODataTypeAnnotation annotation = new(oDataTypeName);
                serializerResult.Resource.TypeAnnotation = annotation;
            }
        }
    }
}
