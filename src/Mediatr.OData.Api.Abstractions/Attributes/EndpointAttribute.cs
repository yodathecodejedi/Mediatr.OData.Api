using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;

namespace Mediatr.OData.Api.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public abstract class EndpointAttribute : Attribute
{
    #region Properties
    public string Route { get; set; } = string.Empty;
    public bool KeyInRoute { get; protected set; } = false;
    public string RouteSegment { get; set; } = string.Empty;
    public EndpointMethod HttpMethod { get; set; } = EndpointMethod.Get;
    public EndpointBinding Binding { get; protected set; } = EndpointBinding.CustomBinding;
    public string? RoutePrefix { get; protected set; } = null;
    public Produces Produces { get; set; } = Produces.IEnumerable;
    #endregion

    #region Constructors
    public EndpointAttribute(EndpointMethod httpMethod)
    {
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(EndpointMethod httpMethod, Produces produces)
    {
        HttpMethod = httpMethod;
        Produces = produces;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod)
    {
        Route = route;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, Produces produces)
    {
        Route = route;
        HttpMethod = httpMethod;
        Produces = produces;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment)
    {
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, Produces produces)
    {
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        Produces = produces;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, Produces produces)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        Produces = produces;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix)
    {
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        RoutePrefix = routePrefix;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces)
    {
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        RoutePrefix = routePrefix;
        Produces = produces;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        RoutePrefix = routePrefix;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces)
    {
        Route = route;
        RouteSegment = routeSegment;
        HttpMethod = httpMethod;
        RoutePrefix = routePrefix;
        Produces = produces;
    }
    #endregion
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EndpointAttribute<TDomainObject> : EndpointAttribute
    where TDomainObject : class, IDomainObject
{
    public Type DomainObjectType => typeof(TDomainObject);

    public EndpointAttribute(EndpointMethod httpMethod) : base(httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(EndpointMethod httpMethod, Produces produces) : base(httpMethod, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, Produces produces) : base(route, httpMethod, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment) : base(httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, Produces produces) : base(httpMethod, routeSegment, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment) : base(route, httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        Produces = Produces.IEnumerable;
    }
    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, Produces produces) : base(route, httpMethod, routeSegment, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces) : base(httpMethod, routeSegment, routePrefix, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(route, httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces) : base(route, httpMethod, routeSegment, routePrefix, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
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
        Produces = Produces.Object;
    }

    public EndpointAttribute(EndpointMethod httpMethod, Produces produces) : base(httpMethod, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.Object;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, Produces produces) : base(route, httpMethod, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment) : base(httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.Object;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, Produces produces) : base(httpMethod, routeSegment, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment) : base(route, httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.Object;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, Produces produces) : base(route, httpMethod, routeSegment, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.Object;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces) : base(httpMethod, routeSegment, routePrefix, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(route, httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.Object;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces) : base(route, httpMethod, routeSegment, routePrefix, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
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
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(EndpointMethod httpMethod, Produces produces) : base(httpMethod, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod) : base(route, httpMethod)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, Produces produces) : base(route, httpMethod, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment) : base(httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, Produces produces) : base(httpMethod, routeSegment, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment) : base(route, httpMethod, routeSegment)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.IEnumerable;
    }
    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, Produces produces) : base(route, httpMethod, routeSegment, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces) : base(httpMethod, routeSegment, routePrefix, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix) : base(route, httpMethod, routeSegment, routePrefix)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
        Produces = Produces.IEnumerable;
    }

    public EndpointAttribute(string route, EndpointMethod httpMethod, string routeSegment, string routePrefix, Produces produces) : base(route, httpMethod, routeSegment, routePrefix, produces)
    {
        Binding = EndpointBinding.DomainObjectBinding;
        KeyInRoute = true;
    }
}