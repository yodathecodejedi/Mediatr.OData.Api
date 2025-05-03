using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Extensions;
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
    public required Produces Produces { set; get; } = Produces.IEnumerable;
    public required Type HandlerType { set; get; } = default!;
    public required ServiceDescriptor ServiceDescriptor { set; get; } = default!;
    public required IAuthorizeData AuthorizeData { set; get; } = default!;

    internal static EndpointMetadata Create(Type targetType, EndpointAttribute endpointAttribute)
    {
        //Determin the information for the root endpoint
        targetType.TryGetGroupRoute(out var groupRoute);
        endpointAttribute.TryGetRoute(out var route);
        //Determin the DomainObject
        endpointAttribute.TryGetDomainObjectType(out var domainObjectType);
        //Detarmin the NavigationObject if it is defined
        endpointAttribute.TryGetNavigationObjectType(out var navigationObjectType);
        //Determin the HttpMethod
        endpointAttribute.TryGetHttpMethod(out var httpMethod);
        //Check if the key is in the route
        endpointAttribute.TryGetKeyInRoute(out var keyInRoute);
        //And determin the binding for the correct endpoint
        endpointAttribute.TryGetBinding(out var binding);
        //Get the KeyType if necessary
        endpointAttribute.TryGetKeyType(out var keyType);
        //Get the routesegment
        domainObjectType.TryGetRouteSegment(navigationObjectType, out var routeSegment);
        //Get the Produces
        domainObjectType.TryGetProduces(navigationObjectType, keyInRoute, httpMethod, out var produces);

        //Get the implementation Interface
        var implementationInterface = TryGetImplementationInterface(httpMethod, keyType, navigationObjectType);
        //Determin the autorization
        var endpointAuthorizeAttribute = targetType.GetCustomAttribute<ODataAuthorizeAttribute>() ?? default!;
        var domainObjectAuthorizeAttribute = domainObjectType.GetCustomAttribute<ODataAuthorizeAttribute>() ?? default!;
        IAuthorizeData authorizeData = (IAuthorizeData)endpointAuthorizeAttribute ?? (IAuthorizeData)domainObjectAuthorizeAttribute ?? default!;

        if (string.IsNullOrEmpty(route) && string.IsNullOrEmpty(groupRoute))
            throw new MissingMemberException($"The endpoint route/grouping is not declared in {targetType.FullName} or its parent. Please use EndpointAttribute on {targetType.FullName} or EndpointGroupAttribute on its parent.");

        var serviceType = TryGetServiceType(httpMethod, domainObjectType, keyType, navigationObjectType);
        if (serviceType == default!)
            throw new MissingMemberException($"The serviceType for {domainObjectType?.Name} and httpMethod {httpMethod} is not found, your endpointhandler is malformatted.");
        var serviceDescriptor = new ServiceDescriptor(serviceType, targetType, ServiceLifetime.Transient) ?? default!;

        return new EndpointMetadata
        {
            AuthorizeData = authorizeData ?? default!,
            Binding = binding,
            DomainObjectType = domainObjectType ?? default!,
            HandlerType = implementationInterface,
            HttpMethod = httpMethod,
            KeyInRoute = keyInRoute,
            KeyType = keyType,
            Produces = produces,
            NavigationObjectType = navigationObjectType ?? default!,
            Route = string.IsNullOrEmpty(route) ? groupRoute : route,
            RouteSegment = routeSegment,
            ServiceDescriptor = serviceDescriptor
        } ?? default!;
    }

    private static Type TryGetServiceType(EndpointMethod httpMethod, Type? domainObjectType, Type keyType, Type? navigationObjectType)
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
            return typeof(IEndpointGetByNavigationHandler<,,>).MakeGenericType(domainObjectType, keyType, navigationObjectType);
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
            return typeof(IEndpointGetByNavigationHandler<,,>);
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


