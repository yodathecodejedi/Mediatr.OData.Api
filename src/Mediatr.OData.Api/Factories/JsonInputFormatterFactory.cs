using Mediatr.OData.Api.Formatters;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData;

namespace Mediatr.OData.Api.Factories;

public static class JsonInputFormatterFactory
{
    private const string JsonMediaType = "application/json";
    private static readonly IEnumerable<ODataPayloadKind> DefaultODataPayloadKinds = [
        ODataPayloadKind.ResourceSet,
        ODataPayloadKind.Resource,
        ODataPayloadKind.Property,
        ODataPayloadKind.EntityReferenceLink,
        ODataPayloadKind.EntityReferenceLinks,
        ODataPayloadKind.Collection,
        ODataPayloadKind.ServiceDocument,
        ODataPayloadKind.Error,
        ODataPayloadKind.Parameter,
        ODataPayloadKind.Delta];

    public static JsonInputFormatter Create()
    {
        return new JsonInputFormatter(DefaultODataPayloadKinds, DefaultInputFormatter());
    }

    public static ODataInputFormatter DefaultInputFormatter()
    {
        return StandardInputFormatters().First(f => f.SupportedMediaTypes.Contains(JsonMediaType));
    }

    public static IList<ODataInputFormatter> CreateAndAdd()
    {
        var formatter = Create();
        var formatters = StandardInputFormatters();
        formatters.Insert(0, formatter);
        return formatters;
    }

    public static List<ODataInputFormatter> StandardInputFormatters()
    {
        return [.. ODataInputFormatterFactory.Create().Reverse()];
    }
}
