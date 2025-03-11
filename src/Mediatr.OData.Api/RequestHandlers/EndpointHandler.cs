using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Metadata;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Mediatr.OData.Api.RequestHandlers;


public class EndpointHandler<TDomainObject>(IConfiguration configuration, ODataMetadataContainer container
        , EndpointMetadata metadata) : IHttpRequestHandler
    where TDomainObject : class, IDomainObject
{
    public Task MapRoutes(WebApplication webApplication)
    {

        //We don't need to use the route since it is part of the Group
        var entityGroup = container.CreateOrGetEndpointGroup(webApplication, metadata);

        RouteHandlerBuilder? routeHandlerBuilder = null;
        if (metadata.HttpMethod == EndpointMethod.Get)
        {
            var route = metadata.Route;
            var routeSegment = metadata.RouteSegment;
            route = routeSegment.IsNullOrWhiteSpace() ? "/" : $"/{routeSegment}";
            routeHandlerBuilder = entityGroup.MapGet(route, async (HttpContext httpContext
                   , [FromServices] IEndpointGetHandler<TDomainObject> handler
                   , [FromQuery] int? PageSize
                   , CancellationToken cancellationToken) =>
            {
                var feature = AddODataFeature(httpContext);

                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(configuration, odataQueryContext, httpContext.Request);

                var result = await handler.Handle(opdataQueryOptions, cancellationToken);
                return result.ToODataResults();
            })
            //.WithSummary($"Endpoint Get Summary")
            //.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
            //.WithDescription("Endpoint get Description")
            .Produces<ODataQueryResult<TDomainObject>>();
        }

        if (metadata.HttpMethod == EndpointMethod.Post)
        {
            var route = metadata.Route;
            var routeSegment = metadata.RouteSegment;
            route = routeSegment.IsNullOrWhiteSpace() ? "/" : $"/{routeSegment}";
            routeHandlerBuilder = entityGroup.MapPost(route, async (HttpRequest httpRequest
                , ODataModel<TDomainObject, Delta<TDomainObject>> domainObjectDelta
                , [FromServices] IEndpointPostHandler<TDomainObject> handler
                , CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(domainObjectDelta.Value!, cancellationToken);
                    return result.ToODataResults();
                })
            //.WithSummary($"Endpoint Get Summary")
            //.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
            //.WithDescription("Endpoint get Description")
            .Produces<ODataQueryResult<TDomainObject>>();

        }

        if (routeHandlerBuilder is null)
            throw new InvalidOperationException($"No request handler found for method {metadata.ServiceDescriptor}");

        if (metadata.AuthorizeData is not null)
        {
            routeHandlerBuilder = routeHandlerBuilder.RequireAuthorization(metadata.AuthorizeData);
        }
        return Task.CompletedTask;
    }

    public static IODataFeature AddODataFeature(HttpContext httpContext)
    {
        var container = (httpContext.GetEndpoint()?.Metadata.OfType<ODataMetadataContainer>().SingleOrDefault()) ?? throw new InvalidOperationException("ODataMetadataContainer not found");
        var odataOptions = httpContext.RequestServices.GetRequiredService<IOptions<ODataOptions>>().Value;

        var entityName = httpContext.GetEndpoint()?.Metadata.OfType<EndpointMetadata>().SingleOrDefault()?.Route ?? throw new InvalidOperationException("Route not found");

        IEdmNavigationSource edmNavigationSource = container.EdmModel.FindDeclaredNavigationSource(entityName);

        var edmEntitySet = container.EdmModel.EntityContainer.FindEntitySet(entityName);
        var entitySetSegment = new EntitySetSegment(edmEntitySet);
        var segments = new List<ODataPathSegment> { entitySetSegment };

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

public class EndpointHandler<TDomainObject, TKey>(IConfiguration configuration, ODataMetadataContainer container
        , EndpointMetadata metadata) : IHttpRequestHandler
    where TDomainObject : class, IDomainObject<TKey>
    where TKey : notnull
{
    public Task MapRoutes(WebApplication webApplication)
    {

        //We don't need to use the route since it is part of the Group
        var entityGroup = container.CreateOrGetEndpointGroup(webApplication, metadata);
        var route = metadata.Route;
        var routeSegment = metadata.RouteSegment;
        route = routeSegment.IsNullOrWhiteSpace() ? "/{key}" : $"/{routeSegment}";

        RouteHandlerBuilder? routeHandlerBuilder = null;
        if (metadata.HttpMethod == EndpointMethod.Get)
        {
            routeHandlerBuilder = entityGroup.MapGet(route, async (HttpContext httpContext
                                , [FromServices] IEndpointGetByKeyHandler<TDomainObject, TKey> handler
                                 , [FromQuery] int? PageSize
                                , TKey key
                                , CancellationToken cancellationToken) =>
            {

                var feature = AddODataFeature(httpContext);

                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(configuration, odataQueryContext, httpContext.Request);

                var result = await handler.Handle(key, opdataQueryOptions, cancellationToken);
                return result.ToODataResults();
            })
            //.WithSummary($"Endpoint Get Summary")
            //.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
            //.WithDescription("Endpoint get Description")
            .Produces<ODataQueryResult<TDomainObject>>();
        }

        if (metadata.HttpMethod == EndpointMethod.Delete)
        {
            routeHandlerBuilder = entityGroup.MapDelete(route, async (HttpContext httpContext
                            , TKey key
                            , [FromServices] IEndpointDeleteHandler<TDomainObject, TKey> handler
                            , CancellationToken cancellationToken) =>
            {
                var feature = AddODataFeature(httpContext);

                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(configuration, odataQueryContext, httpContext.Request);
                var result = await handler.Handle(key, cancellationToken);
                return result.ToODataResults();
            })
            //.WithSummary($"Endpoint Get Summary")
            //.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
            //.WithDescription("Endpoint get Description")
            .Produces<ODataQueryResult<TDomainObject>>();
        }

        if (metadata.HttpMethod == EndpointMethod.Patch)
        {
            routeHandlerBuilder = entityGroup.MapPatch(route, async (
                HttpContext httpContext
                , ODataModel<TDomainObject, TKey, Delta<TDomainObject>> domainObjectDelta
                , TKey key
                , [FromServices] IEndpointPatchHandler<TDomainObject, TKey> handler
                , CancellationToken cancellationToken) =>
                        {
                            var feature = AddODataFeature(httpContext);
                            var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                            var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(configuration, odataQueryContext, httpContext.Request);

                            var result = await handler.Handle(key, domainObjectDelta.Value!, cancellationToken);
                            return result.ToODataResults();
                        })
            //.WithSummary($"Endpoint Get Summary")
            //.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
            //.WithDescription("Endpoint get Description")
            .Produces<ODataQueryResult<TDomainObject>>();
        }

        if (metadata.HttpMethod == EndpointMethod.Put)
        {
            routeHandlerBuilder = entityGroup.MapPut(route, async (
                   HttpContext httpContext
                   , ODataModel<TDomainObject, TKey, Delta<TDomainObject>> domainObjectDelta
                   , TKey key
                   , [FromServices] IEndpointPutHandler<TDomainObject, TKey> handler
                   , CancellationToken cancellationToken) =>
            {
                var feature = AddODataFeature(httpContext);
                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(configuration, odataQueryContext, httpContext.Request);

                var result = await handler.Handle(key, domainObjectDelta.Value!, cancellationToken);
                return result.ToODataResults();
            })
            //.WithSummary($"Endpoint Get Summary")
            //.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
            //.WithDescription("Endpoint get Description")
            .Produces<ODataQueryResult<TDomainObject>>();
        }

        if (routeHandlerBuilder is null)
            throw new InvalidOperationException($"No request handler found for method {metadata.ServiceDescriptor}");

        if (metadata.AuthorizeData is not null)
        {
            routeHandlerBuilder = routeHandlerBuilder.RequireAuthorization(metadata.AuthorizeData);
        }
        return Task.CompletedTask;
    }

    public static IODataFeature AddODataFeature(HttpContext httpContext)
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

public class EndpointHandler<TDomainObject, TKey, TNavigationObject>(IConfiguration configuration, ODataMetadataContainer container
        , EndpointMetadata metadata) : IHttpRequestHandler
    where TDomainObject : class, IDomainObject
    where TKey : notnull
    where TNavigationObject : class, IDomainObject
{
    public Task MapRoutes(WebApplication webApplication)
    {

        //We don't need to use the route since it is part of the Group
        var entityGroup = container.CreateOrGetEndpointGroup(webApplication, metadata);
        var route = metadata.Route;
        var routeSegment = metadata.RouteSegment;

        //For navigation we need to have a routeSegment that fits the navigation
        ArgumentNullException.ThrowIfNull(routeSegment, nameof(routeSegment));

        route = string.Concat("/{key}/", routeSegment);

        RouteHandlerBuilder? routeHandlerBuilder = null;
        if (metadata.HttpMethod == EndpointMethod.Get)
        {
            routeHandlerBuilder = entityGroup.MapGet(route, async (HttpContext httpContext
                   , [FromServices] IEndpoinGetByNavigationHandler<TDomainObject, TKey, TNavigationObject> handler
                   , TKey key
                   , CancellationToken cancellationToken) =>
            {
                var feature = AddODataFeature(httpContext);

                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TNavigationObject), feature.Path);
                var odataQueryOptions = new ODataQueryOptionsWithPageSize<TNavigationObject>(configuration, odataQueryContext, httpContext.Request);

                var result = await handler.Handle(key, typeof(TDomainObject), odataQueryOptions, cancellationToken);

                return result.ToODataResults();
            })
            //.WithSummary($"Endpoint Get Summary")
            //.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
            //.WithDescription("Endpoint get Description")
            .Produces<ODataQueryResult<TDomainObject>>();
        }

        if (routeHandlerBuilder is null)
            throw new InvalidOperationException($"No request handler found for method {metadata.ServiceDescriptor}");

        if (metadata.AuthorizeData is not null)
        {
            routeHandlerBuilder = routeHandlerBuilder.RequireAuthorization(metadata.AuthorizeData);
        }
        return Task.CompletedTask;
    }

    public static IODataFeature AddODataFeature(HttpContext httpContext)
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