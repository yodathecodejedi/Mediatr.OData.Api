using Microsoft.OData;

namespace Mediatr.OData.Api.Extensions;

public static class ODataPropertyInfoExtensions
{
    public static bool TryAddProperty(this List<ODataPropertyInfo> oDataProperties, ODataPropertyInfo oDataPropertyInfo)
    {
        if (oDataPropertyInfo is null) return false;
        if (oDataProperties.Any(op => op.Name.Equals(oDataPropertyInfo.Name))) return false;
        try
        {
            oDataProperties.Add(oDataPropertyInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
