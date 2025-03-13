namespace Mediatr.OData.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class EndpointRouteAttribute : Attribute
    {
        public string Route { get; set; } = string.Empty;
        public string? RoutePrefix { get; set; } = null;


        public EndpointRouteAttribute() { }

        public EndpointRouteAttribute(string route)
        {
            Route = route;

        }

        public EndpointRouteAttribute(string route, string routePrefix)
        {
            Route = route;
            RoutePrefix = routePrefix;
        }
    }
}
