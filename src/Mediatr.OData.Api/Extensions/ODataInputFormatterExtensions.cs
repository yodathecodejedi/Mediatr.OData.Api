using Mediatr.OData.Api.Abstractions.Enumerations;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using System.Text;

namespace Mediatr.OData.Api.Extensions;

public static class ODataInputFormatterExtensions
{
    public static void AddSupportedEncodings(this ODataInputFormatter inputFormatter)
    {
        ArgumentNullException.ThrowIfNull(inputFormatter, nameof(inputFormatter));
        inputFormatter.SupportedEncodings.Add(Encoding.UTF8);
        inputFormatter.SupportedEncodings.Add(Encoding.Unicode);
    }

    public static void AddSupportedMediaTypes(this ODataInputFormatter inputFormatter)
    {
        ArgumentNullException.ThrowIfNull(inputFormatter, nameof(inputFormatter));

        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;odata.streaming=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;odata.streaming=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;odata.streaming=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;odata.streaming=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;odata.streaming=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;odata.streaming=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.streaming=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.streaming=false");
        inputFormatter.SupportedMediaTypes.Add("application/json");
        inputFormatter.SupportedMediaTypes.Add("application/json-patch+json");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=minimal;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=full;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.metadata=none;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.streaming=true;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.streaming=true;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.streaming=false;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;odata.streaming=false;IEEE754Compatible=true");
        inputFormatter.SupportedMediaTypes.Add("application/json;IEEE754Compatible=false");
        inputFormatter.SupportedMediaTypes.Add("application/json;IEEE754Compatible=true");
    }

    public static bool TryGetOperationType(this InputFormatterResult oDataInputFormatterResult, out HttpOperation operationType)
    {
        ArgumentNullException.ThrowIfNull(oDataInputFormatterResult, nameof(oDataInputFormatterResult));
        operationType = HttpOperation.None;
        if (oDataInputFormatterResult.Model is Delta)
        {
            operationType = HttpOperation.Patch;
            return true;
        }
        if (oDataInputFormatterResult.IsModelSet && oDataInputFormatterResult.Model is not null)
        {
            operationType = HttpOperation.Post;
            return true;
        }
        return false;
    }

    public static HttpOperation GetOperationType(this InputFormatterResult oDataInputFormatterResult)
    {
        ArgumentNullException.ThrowIfNull(oDataInputFormatterResult, nameof(oDataInputFormatterResult));
        return oDataInputFormatterResult.TryGetOperationType(out var operationType) ? operationType : HttpOperation.None;
    }
}
