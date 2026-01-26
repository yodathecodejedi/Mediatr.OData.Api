using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mediatr.OData.Api.SchemaFilters
{
    public class MetadataDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathsToRemove = swaggerDoc.Paths
                      .Where(p => p.Key.EndsWith("/$metadata", StringComparison.OrdinalIgnoreCase))
                      .Select(p => p.Key)
                      .ToList();

            foreach (var path in pathsToRemove)
                swaggerDoc.Paths.Remove(path);
        }
    }
}
