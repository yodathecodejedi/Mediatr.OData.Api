using Mediatr.OData.Api.Metadata;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Mediatr.OData.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static IODataFeature AddODataFeature(this HttpContext httpContext)
        {
            var container = (httpContext.GetEndpoint()?.Metadata.OfType<ODataMetadataContainer>().SingleOrDefault()) ?? throw new InvalidOperationException("ODataMetadataContainer not found");
            var odataOptions = httpContext.RequestServices.GetRequiredService<IOptions<ODataOptions>>().Value;

            var entityName = httpContext.GetEndpoint()?.Metadata.OfType<EndpointMetadata>().SingleOrDefault()?.Route ?? throw new InvalidOperationException("Route not found");

            IEdmNavigationSource edmNavigationSource = container.EdmModel.FindDeclaredNavigationSource(entityName);

            var edmEntitySet = container.EdmModel.EntityContainer.FindEntitySet(entityName);
            var entitySetSegment = new EntitySetSegment(edmEntitySet);
            var segments = new List<ODataPathSegment> { entitySetSegment };


            //Dit stukje voor de Navigation, GetByKey, En Navigation wel nodig (Delete, Put en Patch hebben dit niet nodig)
            if (httpContext.Request.RouteValues.TryGetValue("key", out var key))
            {
                var entityType = edmNavigationSource.EntityType;
                var keyName = edmNavigationSource.EntityType.DeclaredKey.SingleOrDefault();
                keyName ??= edmNavigationSource.EntityType.Key().Single();

                var keySegment = new KeySegment(
                    keys: new Dictionary<string, object> { { keyName.Name, key! } },
                    edmType: entityType,
                    navigationSource: edmNavigationSource);

                segments.Add(keySegment);
            }

            var path = new ODataPath(segments);
            var feature = new ODataFeature
            {
                Path = path,
                Model = container.EdmModel,
                RoutePrefix = container.RoutePrefix
            };

            httpContext.Features.Set<IODataFeature>(feature);
            return feature;
        }
    }
}
