using Mediatr.OData.Api.Attributes;
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

            writer.WriteStartObject();
            foreach (var property in properties)
            {
                var propValue = property.GetValue(value);
                var propName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                writer.WritePropertyName(propName);
                JsonSerializer.Serialize(writer, propValue, property.PropertyType, options);
            }
            writer.WriteEndObject();
        }
    }
}
