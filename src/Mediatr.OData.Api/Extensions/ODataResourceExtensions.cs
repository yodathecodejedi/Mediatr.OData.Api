using Microsoft.OData;

namespace Mediatr.OData.Api.Extensions;

public static class ODataResourceExtensions
{
    public static bool TryGetODataProperty(this ODataResource oDataResource, ODataPropertyInfo? oDataPropertyInfo, out ODataProperty oDataProperty)
    {
        ArgumentNullException.ThrowIfNull(oDataResource, nameof(oDataResource));

        try
        {
            oDataProperty = oDataResource.Properties.FirstOrDefault(p =>
                            p.Name.Equals(oDataPropertyInfo?.Name ?? string.Empty)
                            && ((ODataProperty)p).Value != null)
                            as ODataProperty ?? default!;

            return oDataProperty is not null;
        }
        catch
        {
            oDataProperty = default!;
            return false;
        }
    }

    public static bool ODataPropertyExists(this ODataResource oDataResource, ODataPropertyInfo? oDataPropertyInfo)
    {
        ArgumentNullException.ThrowIfNull(oDataResource, nameof(oDataResource));

        try
        {
            return oDataResource.Properties.Any(p =>
                            p.Name.Equals(oDataPropertyInfo?.Name ?? string.Empty)
                            && ((ODataProperty)p).Value != null);
        }
        catch
        {
            return false;
        }
    }
}
