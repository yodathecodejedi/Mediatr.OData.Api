using Mediatr.OData.Api.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CFW.ODataCore.SchemaFilters;

public class ExcludePropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var excludedProperties = context.Type.GetProperties()
            .Where(p => p.GetCustomAttribute<PropertyInternalAttribute>() != null)
            .Select(p => p.Name);

        var propertiesToRemove = schema.Properties
            .Where(p => excludedProperties.Contains(p.Key, StringComparer.OrdinalIgnoreCase))
            .Select(p => p.Key)
            .ToList();

        foreach (var prop in propertiesToRemove)
        {

            schema.Properties.Remove(prop);
        }
    }
}
