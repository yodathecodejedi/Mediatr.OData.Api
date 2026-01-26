using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace Mediatr.OData.Api.SchemaFilters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;
        
        if (schema is OpenApiSchema concrete)
        {
            if (concrete.Enum is null)
                concrete.Enum = [];
            else 
                concrete.Enum.Clear();
            foreach (var value in Enum.GetNames(context.Type))
            {
                concrete.Enum.Add(JsonValue.Create(value));
            }
            concrete.Type = JsonSchemaType.String;
        }
    }
}
