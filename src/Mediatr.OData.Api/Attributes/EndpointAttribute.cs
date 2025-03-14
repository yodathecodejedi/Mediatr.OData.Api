﻿using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Interfaces;

namespace Mediatr.OData.Api.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public abstract class EndpointAttribute : Attribute
{
    #region Properties
    public string Route { get; set; } = string.Empty;
    public bool KeyInRoute { get; protected set; } = false;
    public string RouteSegment { get; set; } = string.Empty;
    public EndpointMethod HttpMethod { get; set; } = EndpointMethod.Get;
    public EndpointBinding Binding { get; protected set; } = EndpointBinding.CustomBinding;
    public string? RoutePrefix { get; protected set; } = null;
    #endregion

    #region Constructors
    public EndpointAttribute(EndpointMethod httpMethod)
    {
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod)
    {
        Route = route;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment)
    {
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix)
    {
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        RoutePrefix = routePrefix;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        RoutePrefix = routePrefix;
    }
    #endregion
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class EndpointAttribute<TDomainObject> : EndpointAttribute
    where TDomainObject : class, IDomainObject
{
    public Type DomainObjectType => typeof(TDomainObject);

    public EndpointAttribute(EndpointMethod httpMethod) : base(httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment) : base(httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment) : base(route, httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(route, httpMethod, routeSegment, routePrefix)
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

    public EndpointAttribute(EndpointMethod httpMethod) : base(httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment) : base(httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment) : base(route, httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(route, httpMethod, routeSegment, routePrefix)
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

    public EndpointAttribute(EndpointMethod httpMethod) : base(httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment) : base(httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment) : base(route, httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(route, httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }
}