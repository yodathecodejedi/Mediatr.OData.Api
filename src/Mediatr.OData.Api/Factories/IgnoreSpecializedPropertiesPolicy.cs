using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mediatr.OData.Api.Factories;

public class IgnoreSpecializedPropertiesPolicy : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(typeof(CustomJsonConverter<>).MakeGenericType(typeToConvert));
    }

    private class CustomJsonConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var type = typeof(T);
            var properties = type.GetProperties()
                .Where(prop => prop.GetCustomAttribute<PropertyInternalAttribute>() == null);

            if (properties.Count() == 1 && properties.First().Name.Equals("Error"))
            {
                writer.WriteStartObject();
                try
                {
                    var errorDescription = value.GetPropertyValue("Error")?.ToString() ?? default!;
                    writer.WriteString("Error", errorDescription);
                }
                catch { }
                writer.WriteEndObject();
            }
            else if (value is ProblemDetails)
            {
                writer.WriteStartObject();
                try
                {
                    var title = value.GetPropertyValue("Title")?.ToString() ?? default!;
                    writer.WriteString("title", title.ToString());
                    var status = value.GetPropertyValue("Status")?.ToString() ?? default!;
                    writer.WriteString("status", status);
                    var details = value.GetPropertyValue("Detail")?.ToString() ?? default!;
                    writer.WriteString("details", details);
                }
                catch { }
                writer.WriteEndObject();
            }
            else
            {
                //// Add additional properties or custom fields if needed
                writer.WriteStartObject();
                foreach (var property in properties)
                {
                    try
                    {
                        var propName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                        var propValue = property.GetValue(value);
                        writer.WritePropertyName(propName);
                        JsonSerializer.Serialize(writer, propValue, property.PropertyType, options);
                    }
                    catch { }
                }
                writer.WriteEndObject();
            }
        }
    }
}
