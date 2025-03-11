using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Mediatr.OData.Api.Formatters;

public class JsonInputFormatter : ODataInputFormatter
{
    private readonly ODataInputFormatter _defaultODataInputFormatter;

    public JsonInputFormatter(IEnumerable<ODataPayloadKind> payloadKinds, ODataInputFormatter defaultODataInputFormatter)//, IEnumerable<IEdmModel> edmModels)
        : base(payloadKinds)
    {
        ArgumentNullException.ThrowIfNull(payloadKinds, nameof(payloadKinds));
        ArgumentNullException.ThrowIfNull(defaultODataInputFormatter, nameof(defaultODataInputFormatter));

        _defaultODataInputFormatter = defaultODataInputFormatter;
        this.AddSupportedMediaTypes();
        this.AddSupportedEncodings();
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
        var rawJson = await reader.ReadToEndAsync();

        Dictionary<string, string> dateFields = [];
        JsonDocument document = JsonDocument.Parse(rawJson);
        var rootElement = document.RootElement;

        Dictionary<string, string> withoutQuoteFields = [];

        //Zoek all values waar geen " " omheen zit 
        //Plaats ze er in de ruwe json toch omheen 
        //Als voordeel dat bool = true/false of 0/1
        //Enum is tekst of waarde 

        foreach (var property in rootElement.EnumerateObject())
        {
            string fieldName = property.Name;
            string fieldValue = property.Value.ToString();

            if (DateTimeExpressions.ISO8601_T_Value().IsMatch(fieldValue)) // Matches datetime without quotes  "yyyy-MM-ddTHH:mm:ss" exact
            {
                dateFields.Add(fieldName, fieldValue);
                continue;
            }
        }

        //Clean up the JsonDocument
        document.Dispose();

        //If nothing found directly go to the defaultInputFormatter
        if (dateFields.Count == 0)
        {
            var unmodifiedStream = new MemoryStream(Encoding.UTF8.GetBytes(rawJson));
            context.HttpContext.Request.Body = unmodifiedStream;
            context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            return await _defaultODataInputFormatter.ReadRequestBodyAsync(context, encoding);
        }

        //Update the rawJson to be acceptable by the DefaultInputFormatter
        rawJson = DateTimeExpressions.ISO8601_T_Raw().Replace(rawJson, match =>
        {
            // Temporary fix the RawJson so the Default InputFormatter will accept the value before we are going to do our magic
            string matchedDate = match.Value.Trim('"'); // Remove quotes
            return $"\"{matchedDate}Z\"";
        });

        //Place the rawJdos back into the Request.Body
        var modifiedStream = new MemoryStream(Encoding.UTF8.GetBytes(rawJson));
        context.HttpContext.Request.Body = modifiedStream;
        context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

        //Process the DefaultInputFormatter with the updated Json
        var result = await _defaultODataInputFormatter.ReadRequestBodyAsync(context, encoding);

        //Enum values supplied as "number" are Deserialized correctly however the value is not validated agains the Enum values
        //Maybe in modelValidate  (my example was a nullable enum, also check still not nullable Enum
        //Same goes for enum values without quotes
        //And maybe the different boolean values


        foreach (var dateField in dateFields)
        {
            if (result.GetOperationType() == HttpOperation.Patch && result.Model is Delta delta)
            {
                delta.TryGetPropertyType(dateField.Key, out var property);
                Type declaredType = Nullable.GetUnderlyingType(property) ?? property;


                //Parse the DateTime as Invariant
                DateTime dateTime = DateTime.ParseExact(dateField.Value, "yyyy-MM-ddTHH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None);
                // Ensure it's set to Local time
                DateTime localDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

                //This is the DateTime part of the InputFormatter for the PostModelSet
                if (declaredType == typeof(DateTime) || declaredType == typeof(DateTimeOffset))
                {
                    if (declaredType == typeof(DateTime))
                        delta.TrySetPropertyValue(dateField.Key, localDateTime);

                    if (declaredType == typeof(DateTimeOffset))
                        delta.TrySetPropertyValue(dateField.Key, new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime)));
                }
            }
            if (result.GetOperationType() == HttpOperation.Post && result.IsModelSet && result.Model is not null)
            {
                if (result.Model.GetType().GetProperty(dateField.Key)?.PropertyType is not Type declaredType) { continue; }
                declaredType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

                //This is the DateTime part of the InputFormatter for the PostModelSet
                if (declaredType == typeof(DateTime) || declaredType == typeof(DateTimeOffset))
                {
                    //Parse the DateTime as Invariant
                    DateTime dateTime = DateTime.ParseExact(dateField.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    // Ensure it's set to Local time
                    DateTime localDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
                    if (declaredType == typeof(DateTime))
                        result.Model.SetPropertyValue(dateField.Key, localDateTime);
                    if (declaredType == typeof(DateTimeOffset))
                        result.Model.SetPropertyValue(dateField.Key, new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime)));
                }
            }
        }

        return result;
    }
}
