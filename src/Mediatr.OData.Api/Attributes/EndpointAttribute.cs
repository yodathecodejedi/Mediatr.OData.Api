using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Interfaces;

namespace Mediatr.OData.Api.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public abstract class EndpointAttribute : Attribute
{
    public string Route { get; set; } = string.Empty;
    public bool KeyInRoute { get; protected set; } = false;
    public string RouteSegment { get; set; } = string.Empty;
    public EndpointMethod HttpMethod { get; set; } = EndpointMethod.Get;
    public EndpointBinding Binding { get; protected set; } = EndpointBinding.CustomBinding;
    public string? RoutePrefix { get; protected set; } = null;


    public EndpointAttribute() { }

    public EndpointAttribute(string route)
    {
        Route = route;
    }

    public EndpointAttribute(string route, string routeSegment)
    {
        Route = route;
        RouteSegment = routeSegment;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod)
    {
        Route = route;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod, string routePrefix)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        RoutePrefix = routePrefix;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class EndpointAttribute<TDomainObject> : EndpointAttribute
    where TDomainObject : class, IDomainObject
{
    public Type DomainObjectType => typeof(TDomainObject);

    public EndpointAttribute(string route, string routeSegment) : base(route, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod) : base(route, routeSegment, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod, string routePrefix) : base(route, routeSegment, httpMethod, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class EndpointAttribute<TDomainObject, Tkey> : EndpointAttribute
    where TDomainObject : class, IDomainObject<Tkey>
    where Tkey : notnull

{
    public Type DomainObjectType => typeof(TDomainObject);
    public Type KeyType => typeof(Tkey);

    public EndpointAttribute(string route, string routeSegment) : base(route, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod) : base(route, routeSegment, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod, string routePrefix) : base(route, routeSegment, httpMethod, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class EndpointAttribute<TDomainObject, Tkey, TNavigationObject> : EndpointAttribute
    where TDomainObject : class, IDomainObject<Tkey>
    where Tkey : notnull
    where TNavigationObject : class, IDomainObject
{
    public Type DomainObjectType => typeof(TDomainObject);
    public Type KeyType => typeof(Tkey);
    public Type NavigationObjectType => typeof(TNavigationObject);

    public EndpointAttribute(string route, string routeSegment) : base(route, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod) : base(route, routeSegment, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, string routeSegment, EndpointMethod httpMethod, string routePrefix) : base(route, routeSegment, httpMethod, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }
}