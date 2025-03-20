using Mediatr.OData.Api.Models;
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

            //This should be easier (some extentionmethod)
            //TODO : Add a method to get the type from the namespace
            var oDataConfiguration = AppContext.GetData("ODataConfiguration") as ODataConfiguration;
            var typeRoot = string.IsNullOrWhiteSpace(oDataConfiguration?.TypeDefinition.Root) ? "odata" : oDataConfiguration.TypeDefinition.Root;
            var firstSegment = oDataConfiguration?.TypeDefinition.FirstSegment ?? default;
            var typeName = resourceContext.ResourceInstance.GetType().Name ?? default!;
            var fullName = resourceContext.ResourceInstance.GetType().FullName ?? default!;
            if (!string.IsNullOrWhiteSpace(firstSegment) && fullName.IndexOf(firstSegment) != -1)
            {
                typeName = fullName.Substring(fullName.IndexOf(firstSegment));
            }
            if (!string.IsNullOrEmpty(typeName))
            {
                var odataTypename = $"{typeRoot}.{typeName}";
                ODataTypeAnnotation annotation = new(odataTypename);
                serializerResult.Resource.TypeAnnotation = annotation;
            }
        }
    }
}
