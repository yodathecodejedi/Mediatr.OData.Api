using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Metadata;
using Mediatr.OData.Api.RequestHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Data;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Mediatr.OData.Api.Models;

public class ODataMetadataContainer(string routePrefix)
{
    public string RoutePrefix { get; } = routePrefix;

    private IEdmModel? _edmModel;
    public IEdmModel EdmModel
    {
        get
        {
            if (_edmModel is null)
                throw new InvalidOperationException("Edm model not build yet");
            return _edmModel;
        }
    }

    private RouteGroupBuilder? _routeGroupBuilder;

    internal RouteGroupBuilder CreateOrGetContainerRoutingGroup(WebApplication app)
    {
        _routeGroupBuilder ??= app.MapGroup(RoutePrefix).WithMetadata(this);

        return _routeGroupBuilder;
    }

    private readonly Dictionary<string, RouteGroupBuilder> _endpointRouteGroupBuider = [];

    public RouteGroupBuilder CreateOrGetEndpointGroup(WebApplication app, EndpointMetadata metadata)
    {
        if (!_endpointRouteGroupBuider.TryGetValue(metadata.Route, out var group))
        {
            group = CreateOrGetContainerRoutingGroup(app)
                .MapGroup(metadata.Route)
                .WithTags(metadata.Route)
                .WithMetadata(metadata);
            _endpointRouteGroupBuider.Add(metadata.Route, group);
        }
        return group;
    }
    private readonly Dictionary<EndpointKey, EndpointMetadata> _endpointMetadata = [];

    internal EndpointMetadata CreateOrEditEndpointMetadata(Type targetType, EndpointAttribute endpointAttribute)
    {
        var endpointMetaData = EndpointMetadata.Create(targetType, endpointAttribute);

        var endpointKey = new EndpointKey
        {
            HttpMethod = endpointMetaData.HttpMethod,
            Route = endpointMetaData.Route,
            RouteSegment = endpointMetaData.RouteSegment,
            KeyType = endpointMetaData.KeyType,
            KeyInRoute = endpointMetaData.KeyInRoute,
            ObjectType = endpointMetaData.DomainObjectType
        };

        if (!_endpointMetadata.TryGetValue(endpointKey, out EndpointMetadata? configuredMetadata))
        {
            configuredMetadata = endpointMetaData;
            _endpointMetadata.Add(endpointKey, configuredMetadata);
            return endpointMetaData;
        }

        return configuredMetadata;
    }

    internal void BuildEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        foreach (var (endpointKey, metadata) in _endpointMetadata)
        {
            var objectProperties = endpointKey.ObjectType.GetProperties();
            var oDataEntityType = modelBuilder.AddEntityType(endpointKey.ObjectType);
            var entitySetConfiguration = modelBuilder.AddEntitySet(endpointKey.Route, oDataEntityType);

            //Find the properties that should be ignored on the EDM Model (like the E-tag)
            var propertiesToBeIgnored = objectProperties.Where(prop =>
                prop.GetCustomAttributes<PropertyModeAttribute>().Any(t => t.Mode == Mode.ETag || t.Mode == Mode.Hash) ||
                prop.GetCustomAttributes<JsonIgnoreAttribute>().Any() ||
                prop.GetCustomAttributes<PropertyHashAttribute>().Any() ||
                prop.GetCustomAttributes<ODataETagAttribute>().Any() ||
                prop.GetCustomAttributes<PropertyInternalAttribute>().Any()
            );

            foreach (var propertyToBeIgnored in propertiesToBeIgnored)
            {
                //Remove the property from the EDM model, so it is not returned from OData
                entitySetConfiguration.EntityType.RemoveProperty(propertyToBeIgnored);
            }

            var enumProperties = objectProperties
                .Where(x => x.PropertyType.IsEnum)
                .ToList();
            foreach (var property in enumProperties)
            {
                modelBuilder.AddEnumType(property.PropertyType);
            }
        }

        _edmModel = modelBuilder.GetEdmModel();
        _edmModel.MarkAsImmutable();
    }

    internal void RegisterRoutingServices(IServiceCollection services)
    {
        var allServiceDescriptors = _endpointMetadata.Values.Select(emd => emd.ServiceDescriptor);
        foreach (var serviceDescriptor in allServiceDescriptors)
        {
            services.Add(serviceDescriptor);
        }

        var requestHandlerType = typeof(IHttpRequestHandler);
        foreach (var (endpointKey, metadata) in _endpointMetadata)
        {
            var KeyType = metadata.KeyType;
            var domainObjectType = metadata.DomainObjectType;
            var navigationObjectType = metadata.NavigationObjectType;

            if (domainObjectType is not null && KeyType is null && navigationObjectType is null)
            {
                var endpointHandlerType = typeof(EndpointHandler<>).MakeGenericType(metadata.DomainObjectType);
                services.AddSingleton(requestHandlerType, s
                    => ActivatorUtilities.CreateInstance(s, endpointHandlerType, this, metadata));
            }
            if (domainObjectType is not null && KeyType is not null && navigationObjectType is null)
            {
                var endpointHandlerType = typeof(EndpointHandler<,>).MakeGenericType(metadata.DomainObjectType, metadata.KeyType);
                services.AddSingleton(requestHandlerType, s
                    => ActivatorUtilities.CreateInstance(s, endpointHandlerType, this, metadata));
            }
            if (domainObjectType is not null && KeyType is not null && navigationObjectType is not null)
            {
                var endpointHandlerType = typeof(EndpointHandler<,,>).MakeGenericType(metadata.DomainObjectType, metadata.KeyType, metadata.NavigationObjectType);
                services.AddSingleton(requestHandlerType, s
                    => ActivatorUtilities.CreateInstance(s, endpointHandlerType, this, metadata));
            }
        }
    }
}


