using Mediatr.OData.Api.Abstractions.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Mediatr.OData.Api.SchemaFilters;

public class ExcludePropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var excludedProperties = context.Type.GetProperties()
            .Where(p =>
                p.Name.Equals("Hash") || p.Name.Equals("ETag") ||
                p.GetCustomAttribute<InternalAttribute>() != null ||
                p.GetCustomAttribute<HashAttribute>() != null ||
                p.GetCustomAttribute<InternalKeyAttribute>() != null ||
                p.GetCustomAttribute<ODataETagAttribute>() != null
            )
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
