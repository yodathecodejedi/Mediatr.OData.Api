using Mediatr.OData.Api.Abstractions.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Mediatr.OData.Api.SchemaFilters;

public class ExcludePropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        //Exclude properties that are not relevant for the OData API Model
        var excludedProperties = context.Type.GetProperties()
            .Where(p =>
                //Based on the implicit name of the property ETag or Hash
                p.Name.Equals("Hash") || p.Name.Equals("ETag") ||
                //Based on the attributes
                p.GetCustomAttribute<ODataIgnoreAttribute>() != null ||
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
