﻿using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Metadata;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;

namespace Mediatr.OData.Api.RequestHandlers;

//TODO:
//.WithSummary($"Endpoint Get Summary")
//.WithDisplayName($"endpoint Get DisplayName  {metadata.Name}")
//.WithDescription("Endpoint get Description")
public sealed class EndpointHandler<TDomainObject>(ODataMetadataContainer container
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
                var feature = httpContext.AddODataFeature();
                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                //Hier dus een Factory voor maken omdat het anders niet werkt
                var odataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);

                var result = await handler.Handle(odataQueryOptions, cancellationToken);
                return result.ToODataResults();
            })
            .ApplyIf(metadata.Produces == Produces.Object, rhb => rhb.Produces<TDomainObject>())
            .ApplyIf(metadata.Produces == Produces.IEnumerable, rhb => rhb.Produces<IEnumerable<TDomainObject>>())
            .ApplyIf(metadata.Produces == Produces.Value, rhb => rhb.Produces<int>());

            //Special endpoint for count
            if (metadata.Produces == Produces.IEnumerable)
            {
                var routeCount = route.EndsWith('/') ? $"{route}$count" : $"{route}/$count";
                routeHandlerBuilder = entityGroup.MapGet(routeCount, async (HttpContext httpContext
                    , [FromServices] IEndpointGetHandler<TDomainObject> handler
                    , [FromQuery] int? PageSize
                    , CancellationToken cancellationToken) =>
                    {
                        var feature = httpContext.AddODataFeature();
                        var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                        var odataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);

                        var result = await handler.Handle(odataQueryOptions, cancellationToken);
                        return result.ToODataResults();
                    })
                .Produces<int>();
            }
        }

        if (metadata.HttpMethod == EndpointMethod.Post)
        {
            var route = metadata.Route;
            var routeSegment = metadata.RouteSegment;
            route = routeSegment.IsNullOrWhiteSpace() ? "/" : $"/{routeSegment}";
            routeHandlerBuilder = entityGroup.MapPost(route, async (HttpContext httpContext
                , ODataModel<TDomainObject, Delta<TDomainObject>> domainObjectDelta
                , [FromServices] IEndpointPostHandler<TDomainObject> handler
                , [FromQuery] int? PageSize
                , CancellationToken cancellationToken) =>
            {
                var feature = httpContext.AddODataFeature();
                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var odataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);

                var result = await handler.Handle(domainObjectDelta.Value!, odataQueryOptions, cancellationToken);
                return result.ToODataResults();
            })
            .ApplyIf(metadata.Produces == Produces.Object, rhb => rhb.Produces<TDomainObject>())
            .ApplyIf(metadata.Produces == Produces.IEnumerable, rhb => rhb.Produces<IEnumerable<TDomainObject>>())
            .ApplyIf(metadata.Produces == Produces.Value, rhb => rhb.Produces<int>());

        }

        if (routeHandlerBuilder is null)
            throw new InvalidOperationException($"No request handler found for method {metadata.ServiceDescriptor}");

        if (metadata.AuthorizeData is not null)
        {
            routeHandlerBuilder = routeHandlerBuilder.RequireAuthorization(metadata.AuthorizeData);
        }
        return Task.CompletedTask;
    }
}

public sealed class EndpointHandler<TDomainObject, TKey>(ODataMetadataContainer container
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
                var feature = httpContext.AddODataFeature();
                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);

                var result = await handler.Handle(key, opdataQueryOptions, cancellationToken);
                return result.ToODataResults();
            })
            .ApplyIf(metadata.Produces == Produces.Object, rhb => rhb.Produces<TDomainObject>())
            .ApplyIf(metadata.Produces == Produces.IEnumerable, rhb => rhb.Produces<IEnumerable<TDomainObject>>())
            .ApplyIf(metadata.Produces == Produces.Value, rhb => rhb.Produces<int>());

            //Special endpoint for count
            if (metadata.Produces == Produces.IEnumerable)
            {
                var routeCount = route.EndsWith('/') ? $"{route}$count" : $"{route}/$count";
                routeHandlerBuilder = entityGroup.MapGet(route, async (HttpContext httpContext
                                                , [FromServices] IEndpointGetByKeyHandler<TDomainObject, TKey> handler
                                                 , [FromQuery] int? PageSize
                                                , TKey key
                                                , CancellationToken cancellationToken) =>
                {
                    var feature = httpContext.AddODataFeature();
                    var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                    var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);

                    var result = await handler.Handle(key, opdataQueryOptions, cancellationToken);
                    return result.ToODataResults();
                })
                .Produces<int>();
            }
        }

        if (metadata.HttpMethod == EndpointMethod.Delete)
        {
            routeHandlerBuilder = entityGroup.MapDelete(route, async (HttpContext httpContext
                            , TKey key
                            , [FromServices] IEndpointDeleteHandler<TDomainObject, TKey> handler
                            , CancellationToken cancellationToken) =>
            {
                var feature = httpContext.AddODataFeature();
                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var opdataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);
                var result = await handler.Handle(key, cancellationToken);
                return result.ToODataResults();
            })
            //This might be wrong and actually returns succes or true like with Count (new response model)
            .ApplyIf(metadata.Produces == Produces.Object, rhb => rhb.Produces<TDomainObject>())
            .ApplyIf(metadata.Produces == Produces.IEnumerable, rhb => rhb.Produces<IEnumerable<TDomainObject>>())
            .ApplyIf(metadata.Produces == Produces.Value, rhb => rhb.Produces<int>());
        }

        if (metadata.HttpMethod == EndpointMethod.Patch)
        {
            routeHandlerBuilder = entityGroup.MapPatch(route, async (
                HttpContext httpContext
                , ODataModel<TDomainObject, TKey, Delta<TDomainObject>> domainObjectDelta
                , TKey key
                , [FromServices] IEndpointPatchHandler<TDomainObject, TKey> handler
                , [FromQuery] int? PageSize
                , CancellationToken cancellationToken) =>
                        {
                            var feature = httpContext.AddODataFeature();
                            var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                            var oDataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);

                            var result = await handler.Handle(key, domainObjectDelta.Value!, oDataQueryOptions, cancellationToken);
                            return result.ToODataResults();
                        })
            .ApplyIf(metadata.Produces == Produces.Object, rhb => rhb.Produces<TDomainObject>())
            .ApplyIf(metadata.Produces == Produces.IEnumerable, rhb => rhb.Produces<IEnumerable<TDomainObject>>())
            .ApplyIf(metadata.Produces == Produces.Value, rhb => rhb.Produces<int>());
        }

        if (metadata.HttpMethod == EndpointMethod.Put)
        {
            routeHandlerBuilder = entityGroup.MapPut(route, async (
                   HttpContext httpContext
                   , ODataModel<TDomainObject, TKey, Delta<TDomainObject>> domainObjectDelta
                   , TKey key
                   , [FromServices] IEndpointPutHandler<TDomainObject, TKey> handler
                   , [FromQuery] int? PageSize
                   , CancellationToken cancellationToken) =>
            {
                var feature = httpContext.AddODataFeature();
                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TDomainObject), feature.Path);
                var odataQueryOptions = new ODataQueryOptionsWithPageSize<TDomainObject>(odataQueryContext, httpContext.Request);

                var result = await handler.Handle(key, domainObjectDelta.Value!, odataQueryOptions, cancellationToken);
                return result.ToODataResults();
            })
            .ApplyIf(metadata.Produces == Produces.Object, rhb => rhb.Produces<TDomainObject>())
            .ApplyIf(metadata.Produces == Produces.IEnumerable, rhb => rhb.Produces<IEnumerable<TDomainObject>>())
            .ApplyIf(metadata.Produces == Produces.Value, rhb => rhb.Produces<int>());
        }

        if (routeHandlerBuilder is null)
            throw new InvalidOperationException($"No request handler found for method {metadata.ServiceDescriptor}");

        if (metadata.AuthorizeData is not null)
        {
            routeHandlerBuilder = routeHandlerBuilder.RequireAuthorization(metadata.AuthorizeData);
        }
        return Task.CompletedTask;
    }
}

public sealed class EndpointHandler<TDomainObject, TKey, TNavigationObject>(ODataMetadataContainer container
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
                   , [FromServices] IEndpointGetByNavigationHandler<TDomainObject, TKey, TNavigationObject> handler
                   , [FromQuery] int? PageSize
                   , TKey key
                   , CancellationToken cancellationToken) =>
            {
                var feature = httpContext.AddODataFeature();
                var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TNavigationObject), feature.Path);
                var odataQueryOptions = new ODataQueryOptionsWithPageSize<TNavigationObject>(odataQueryContext, httpContext.Request);

                var result = await handler.Handle(key, typeof(TDomainObject), odataQueryOptions, cancellationToken);

                return result.ToODataResults();
            })
            .ApplyIf(metadata.Produces == Produces.Object, rhb => rhb.Produces<TNavigationObject>())
            .ApplyIf(metadata.Produces == Produces.IEnumerable, rhb => rhb.Produces<IEnumerable<TNavigationObject>>())
            .ApplyIf(metadata.Produces == Produces.Value, rhb => rhb.Produces<int>());

            //Add /$count if the produces is IEnumerable
            if (metadata.Produces == Produces.IEnumerable)
            {
                //Special route for /$count
                var routeCount = route.EndsWith('/') ? $"{route}$count" : $"{route}/$count";
                routeHandlerBuilder = entityGroup.MapGet(routeCount, async (HttpContext httpContext
                   , [FromServices] IEndpointGetByNavigationHandler<TDomainObject, TKey, TNavigationObject> handler
                   , [FromQuery] int? PageSize
                   , TKey key
                   , CancellationToken cancellationToken) =>
                        {
                            var feature = httpContext.AddODataFeature();
                            var odataQueryContext = new ODataQueryContext(feature.Model, typeof(TNavigationObject), feature.Path);
                            var odataQueryOptions = new ODataQueryOptionsWithPageSize<TNavigationObject>(odataQueryContext, httpContext.Request);

                            var result = await handler.Handle(key, typeof(TDomainObject), odataQueryOptions, cancellationToken);

                            return result.ToODataResults();
                        })
                .Produces<int>();
            }
        }

        if (routeHandlerBuilder is null)
            throw new InvalidOperationException($"No request handler found for method {metadata.ServiceDescriptor}");

        if (metadata.AuthorizeData is not null)
        {
            routeHandlerBuilder = routeHandlerBuilder.RequireAuthorization(metadata.AuthorizeData);
        }
        return Task.CompletedTask;
    }
}