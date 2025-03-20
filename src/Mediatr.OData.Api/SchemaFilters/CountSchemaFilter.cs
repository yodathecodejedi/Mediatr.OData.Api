using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mediatr.OData.Api.SchemaFilters
{
    public class CountSchemaFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the route ends with "/$count"
            var path = context.ApiDescription.RelativePath ?? default!;

            if (path.EndsWith("/$count", StringComparison.OrdinalIgnoreCase))
            {
                // Hide the endpoint from Swagger UI
                operation.Tags.Clear();  // Removes the tag (also hides the endpoint in UI)
                operation.Responses.Clear(); // Removes responses for this operation
            }
        }
    }
}
