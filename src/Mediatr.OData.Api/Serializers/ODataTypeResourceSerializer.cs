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

        if (resourceContext.StructuredType.TypeKind is Microsoft.OData.Edm.EdmTypeKind.Entity)
        {
            //We are now adding ATG to the typename however normally we should take the domain from the Namespace of the entity
            //Example Dossier -> Dossier.Afdeling -> ATG.Dossier.Afdeling
            var typeName = resourceContext.ResourceInstance.GetType().Name ?? default!;
            if (!string.IsNullOrEmpty(typeName))
            {
                var odataTypename = $"ATG.{typeName}";
                ODataTypeAnnotation annotation = new(odataTypename);
                serializerResult.Resource.TypeAnnotation = annotation;
            }
        }
    }
}
