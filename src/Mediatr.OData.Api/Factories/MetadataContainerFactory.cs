using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediatr.OData.Api.Factories;

public class MetadataContainerFactory
{
    private static readonly List<Type> _cachedEndpointTypes = [.. AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        .SelectMany(a => a.GetTypes())
        .Where(x => x.GetCustomAttributes<EndpointAttribute>().Any())];

    public IEnumerable<Type> CacheEndpointType { protected set; get; } = _cachedEndpointTypes;

    public IEnumerable<ODataMetadataContainer> CreateContainers(IServiceCollection services, string defaultRoutePrefix)
    {
        var endpointRoutingAttributes = CacheEndpointType
            .SelectMany(endpointType => endpointType.GetCustomAttributes<EndpointAttribute>()
            .Select(attr => new { TargetType = endpointType, RoutingAttribute = attr }))
            .GroupBy(endpointType => endpointType.RoutingAttribute.RoutePrefix ?? defaultRoutePrefix)
            .ToDictionary(endpointType => new ODataMetadataContainer(endpointType.Key), endpointType => endpointType.ToList());

        foreach (var (container, endpointInfoContainer) in endpointRoutingAttributes)
        {
            _ = endpointInfoContainer
                .Where(endpointType => endpointType.RoutingAttribute is not null)
                .Aggregate(container, (currentContainer, endpointType) =>
                {
                    currentContainer.CreateOrEditEndpointMetadata(endpointType.TargetType, (EndpointAttribute)endpointType.RoutingAttribute);
                    return currentContainer;
                });
            container.BuildEdmModel();

            container.RegisterRoutingServices(services);

            yield return container;
        }
    }
}
