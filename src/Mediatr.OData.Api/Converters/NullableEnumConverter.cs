using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mediatr.OData.Api.Converters;

public class NullableEnumConverter<T> : JsonConverter<T?> where T : struct, Enum
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumText = reader.GetString();
            if (string.IsNullOrWhiteSpace(enumText))
                return null;

            if (Enum.TryParse(enumText, true, out T result))
                return result;
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException("Invalid value for enum.");
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString());
        else
            writer.WriteNullValue();
    }
}
