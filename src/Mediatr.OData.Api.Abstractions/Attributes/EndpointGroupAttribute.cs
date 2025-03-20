namespace Mediatr.OData.Api.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class EndpointGroupAttribute : Attribute
    {
        public string Route { get; set; } = string.Empty;
        public string? RoutePrefix { get; set; } = null;


        public EndpointGroupAttribute() { }

        public EndpointGroupAttribute(string route)
        {
            Route = route;

        }

        public EndpointGroupAttribute(string route, string routePrefix)
        {
            Route = route;
            RoutePrefix = routePrefix;
        }
    }
}
