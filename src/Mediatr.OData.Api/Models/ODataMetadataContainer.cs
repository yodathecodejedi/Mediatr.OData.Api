using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Extensions;
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
            //The ClrObjectType that is used in the ODataModelBuilder
            Type objectType = endpointKey.ObjectType;
            //The properties of the ClrOjectType
            PropertyInfo[] objectProperties = endpointKey.ObjectType.GetProperties();
            //The TypeConfiguration for the EdmModel
            EntityTypeConfiguration oDataEntityType = modelBuilder.AddEntityType(endpointKey.ObjectType);
            //The SetConfiguration for the EdmModel
            EntitySetConfiguration entitySetConfiguration = modelBuilder.AddEntitySet(endpointKey.Route, oDataEntityType);

            if (!objectType.TryGetKeyProperty(out var keyProperty) && !oDataEntityType.Keys.Any())
            {
                //Missing keyproperty
                throw new MissingPrimaryKeyException($"The key property is not declared in {objectType.FullName}. Please use ODataKeyAttribute on the key property.");
            }
            oDataEntityType.HasKey(keyProperty);

            //Get the properties we need to exclude from the EDM model (e.g. ODataIgnore, ODataETag) 
            var propertiesToBeIgnored = objectProperties
                //Exclude the KeyProperty from the list of properties to be ignored just in case it was an Implicit Key 
                .Where(prop => !prop.Name.Equals(keyProperty.Name))
                //Select the following Attributes ODataIgnore, ODataETag
                .Where(prop =>
                    prop.GetCustomAttributes<ODataETagAttribute>().Any() ||
                    prop.GetCustomAttributes<ODataIgnoreAttribute>().Any());

            foreach (var propertyToBeIgnored in propertiesToBeIgnored)
            {
                //Remove the property from the EDM model, so it is not returned from OData
                entitySetConfiguration.EntityType.RemoveProperty(propertyToBeIgnored);
            }

            //Add the enumaration properties to the EDM model
            var enumProperties = objectProperties
                .Where(x => x.PropertyType.IsEnum)
                .ToList();
            foreach (var property in enumProperties)
            {
                modelBuilder.AddEnumType(property.PropertyType);
            }
        }
        //Build the mopdel
        _edmModel = modelBuilder.GetEdmModel();
        //Mark it is immutable so it can be used in the OData route
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


