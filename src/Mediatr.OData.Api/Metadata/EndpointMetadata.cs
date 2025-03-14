using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediatr.OData.Api.Metadata;

public class EndpointMetadata
{
    public required Type DomainObjectType { set; get; } = default!;
    public required Type KeyType { set; get; } = default!;
    public required Type NavigationObjectType { set; get; } = default!;
    public required string Route { set; get; } = string.Empty;
    public required string RouteSegment { set; get; } = string.Empty;
    public required EndpointMethod HttpMethod { set; get; } = EndpointMethod.Get;
    public required EndpointBinding Binding { set; get; } = EndpointBinding.CustomBinding;
    public required bool KeyInRoute { set; get; } = false;
    public required Type HandlerType { set; get; } = default!;
    public required ServiceDescriptor ServiceDescriptor { set; get; } = default!;
    public required IAuthorizeData AuthorizeData { set; get; } = default!;

    internal static EndpointMetadata Create(Type targetType, EndpointAttribute endpointAttribute)
    {
        targetType.TryGetGroupRoute(out var groupRoute);
        endpointAttribute.TryGetRoute(out var route);
        endpointAttribute.TryGetRouteSegment(out var routeSegment);
        endpointAttribute.TryGetHttpMethod(out var httpMethod);
        endpointAttribute.TryGetKeyInRoute(out var keyInRoute);
        endpointAttribute.TryGetBinding(out var binding);
        endpointAttribute.TryGetDomainObjectType(out var domainObjectType);
        endpointAttribute.TryGetKeyType(out var keyType);
        endpointAttribute.TryGetNavigationObjectType(out var navigationObjectType);

        var implementationInterface = TryGetImplementationInterface(httpMethod, keyType, navigationObjectType);
        var endpointAuthorizeAttribute = targetType.GetCustomAttribute<EndpointAuthorizeAttribute>() ?? default!;
        var domainObjectAuthorizeAttribute = domainObjectType.GetCustomAttribute<ObjectAuthorizeAttribute>() ?? default!;
        IAuthorizeData authorizeData = (IAuthorizeData)endpointAuthorizeAttribute ?? (IAuthorizeData)domainObjectAuthorizeAttribute ?? default!;

        if (string.IsNullOrEmpty(route) && string.IsNullOrEmpty(groupRoute))
            throw new MissingMemberException($"The endpoint route/grouping is not declared in {targetType.FullName} or its parent. Please use EndpointAttribute on {targetType.FullName} or EndpointGroupAttribute on its parent.");

        var serviceType = TryGetServiceType(httpMethod, domainObjectType, keyType, navigationObjectType);
        var serviceDescriptor = new ServiceDescriptor(serviceType, targetType, ServiceLifetime.Transient) ?? default!;

        return new EndpointMetadata
        {
            AuthorizeData = endpointAuthorizeAttribute != default! ? authorizeData : default!,
            Binding = binding,
            DomainObjectType = domainObjectType,
            HandlerType = implementationInterface,
            HttpMethod = httpMethod,
            KeyInRoute = keyInRoute,
            KeyType = keyType,
            NavigationObjectType = navigationObjectType,
            Route = string.IsNullOrEmpty(route) ? groupRoute : route,
            RouteSegment = routeSegment,
            ServiceDescriptor = serviceDescriptor
        } ?? default!;
    }

    private static Type TryGetServiceType(EndpointMethod httpMethod, Type domainObjectType, Type keyType, Type navigationObjectType)
    {
        if (httpMethod == EndpointMethod.Delete && keyType is not null && domainObjectType is not null && navigationObjectType is null)
        {
            return typeof(IEndpointDeleteHandler<,>).MakeGenericType(domainObjectType, keyType);
        }

        if (httpMethod == EndpointMethod.Get && keyType is null && domainObjectType is not null && navigationObjectType is null)
        {
            return typeof(IEndpointGetHandler<>).MakeGenericType(domainObjectType);
        }

        if (httpMethod == EndpointMethod.Get && keyType is not null && domainObjectType is not null && navigationObjectType is null)
        {
            return typeof(IEndpointGetByKeyHandler<,>).MakeGenericType(domainObjectType, keyType);
        }

        if (httpMethod == EndpointMethod.Get && keyType is not null && domainObjectType is not null && navigationObjectType is not null)
        {
            return typeof(IEndpoinGetByNavigationHandler<,,>).MakeGenericType(domainObjectType, keyType, navigationObjectType);
        }

        if (httpMethod == EndpointMethod.Patch && keyType is not null && domainObjectType is not null && navigationObjectType is null)
        {
            return typeof(IEndpointPatchHandler<,>).MakeGenericType(domainObjectType, keyType);
        }

        if (httpMethod == EndpointMethod.Post && domainObjectType is not null && keyType is null && navigationObjectType is null)
        {
            return typeof(IEndpointPostHandler<>).MakeGenericType(domainObjectType);
        }

        if (httpMethod == EndpointMethod.Put && keyType is not null && domainObjectType is not null && navigationObjectType is null)
        {
            return typeof(IEndpointPutHandler<,>).MakeGenericType(domainObjectType, keyType);
        }
        return default!;
    }

    private static Type TryGetImplementationInterface(EndpointMethod method, Type TypeKey, Type NavigationDomainObject)
    {
        if (method == EndpointMethod.Delete && TypeKey is not null && NavigationDomainObject is null)
        {
            return typeof(IEndpointDeleteHandler<,>);
        }
        if (method == EndpointMethod.Get && TypeKey is null && NavigationDomainObject is null)
        {
            return typeof(IEndpointGetHandler<>);
        }
        if (method == EndpointMethod.Get && TypeKey is not null && NavigationDomainObject is null)
        {
            return typeof(IEndpointGetByKeyHandler<,>);
        }
        if (method == EndpointMethod.Get && TypeKey is not null && NavigationDomainObject is not null)
        {
            return typeof(IEndpoinGetByNavigationHandler<,,>);
        }
        if (method == EndpointMethod.Patch && TypeKey is not null && NavigationDomainObject is null)
        {
            return typeof(IEndpointPatchHandler<,>);
        }
        if (method == EndpointMethod.Post && TypeKey is null && NavigationDomainObject is null)
        {
            return typeof(IEndpointPostHandler<>);
        }
        if (method == EndpointMethod.Put && TypeKey is not null && NavigationDomainObject is null)
        {
            return typeof(IEndpointPutHandler<,>);
        }
        return default!;
    }
}


