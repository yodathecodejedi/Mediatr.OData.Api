using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Mediatr.OData.Api.Extensions;

public static class SelectExpandNodeExtensions
{
    public static bool TryGetStructuralProperty(this SelectExpandNode selectExpandNode, ODataPropertyInfo oDataPropertyInfo, EdmTypeKind edmTypeKind, out IEdmStructuralProperty structuralProperty)
    {
        ArgumentNullException.ThrowIfNull(selectExpandNode, nameof(selectExpandNode));

        try
        {
            structuralProperty = selectExpandNode.SelectedStructuralProperties.FirstOrDefault(sp =>
                    sp.Name.Equals(oDataPropertyInfo.Name) &&
                    sp.Type.Definition.TypeKind == edmTypeKind) ?? default!;

            return structuralProperty is not null;
        }
        catch
        {
            structuralProperty = default!;
            return false;
        }
    }

    public static bool PropertyIsOfEdmKind(this SelectExpandNode selectExpandNode, ODataPropertyInfo oDataPropertyInfo, EdmTypeKind edmTypeKind)
    {
        ArgumentNullException.ThrowIfNull(selectExpandNode, nameof(selectExpandNode));

        try
        {
            var count = selectExpandNode.SelectedStructuralProperties.Where(sp => sp.Name.Equals(oDataPropertyInfo.Name) && sp.Type.Definition.TypeKind == edmTypeKind)?.Count() | 0;
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool PropertyIsOfEdmKind(this SelectExpandNode selectExpandNode, ODataPropertyInfo oDataPropertyInfo, string edmTypeKind)
    {
        ArgumentNullException.ThrowIfNull(selectExpandNode, nameof(selectExpandNode));

        if (string.IsNullOrWhiteSpace(edmTypeKind))
            return false;

        try
        {
            var nameParts = edmTypeKind.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (nameParts is null || nameParts.Length == 0) return false;

            var fullName = string.Empty;
            switch (nameParts.Length)
            {
                case 1:
                    fullName = $"Edm.{nameParts[0]}";
                    break;
                case 2:
                    fullName = edmTypeKind;
                    break;
                default:
                    return false;
            }
            var count = selectExpandNode.SelectedStructuralProperties.Where(sp => sp.Name.Equals(oDataPropertyInfo.Name) && (sp.Type.Definition.ToString() ?? String.Empty).Equals(fullName))?.Count() | 0;
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool PropertyIsOfEdmKind(this SelectExpandNode selectExpandNode, ODataPropertyInfo oDataPropertyInfo, Type type)
    {
        ArgumentNullException.ThrowIfNull(selectExpandNode, nameof(selectExpandNode));

        if (type is null)
            return false;

        try
        {
            var count = selectExpandNode.SelectedStructuralProperties.Where(sp => sp.Name.Equals(oDataPropertyInfo.Name) && (sp.Type.Definition.ToString() ?? String.Empty).Equals($"Edm.{type.Name}"))?.Count() | 0;
            return count > 0;
        }
        catch
        {
            return false;
        }
    }
}
