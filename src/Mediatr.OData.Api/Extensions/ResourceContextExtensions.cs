using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Formatter;

namespace Mediatr.OData.Api.Extensions;

public static class ResourceContextExtensions
{

    public static bool TryGetODataTypeName(this ResourceContext resourceContext, out string oDataTypeName)
    {
        //Patterns => {root}.{optionalFirstSegment}.{typeName}
        ArgumentNullException.ThrowIfNull(resourceContext, nameof(resourceContext));
        oDataTypeName = default!;
        var typeName = resourceContext.ResourceInstance.GetType().Name ?? default!;
        var fullName = resourceContext.ResourceInstance.GetType().FullName ?? default!;
        //Reflection failed to get the type name
        if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(fullName))
        {
            return false;
        }

        //Get configuration data
        var oDataConfiguration = AppContext.GetData("ODataConfiguration") as ODataConfiguration;
        //typeroot defaults to "type"
        var typeRoot = string.IsNullOrWhiteSpace(oDataConfiguration?.TypeDefinition.Root) ? "type" : oDataConfiguration.TypeDefinition.Root;
        var firstSegment = oDataConfiguration?.TypeDefinition.FirstSegment ?? default;
        var useFirstSegment = firstSegment != default! && (oDataConfiguration?.TypeDefinition.UseFirstSegment ?? false);

        //FirstSegment was not defined in the configuration or not found in the full name
        if (string.IsNullOrWhiteSpace(firstSegment) || fullName.IndexOf(firstSegment) == -1)
        {
            oDataTypeName = $"{typeRoot}.{typeName}";
            return true;
        }

        //Take the substring including first segment or substring after first segment (also taking the additional . into account)
        var segementIndex = useFirstSegment ? fullName.IndexOf(firstSegment) : fullName.IndexOf(firstSegment) + firstSegment.Length + 1;
        typeName = fullName.Substring(segementIndex);
        oDataTypeName = $"{typeRoot}.{typeName}";
        return true;
    }
}
