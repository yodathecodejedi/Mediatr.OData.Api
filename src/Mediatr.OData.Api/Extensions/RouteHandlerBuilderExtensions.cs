using Microsoft.AspNetCore.Builder;

namespace Mediatr.OData.Api.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder ApplyIf(this RouteHandlerBuilder builder, bool condition, Func<RouteHandlerBuilder, RouteHandlerBuilder> action)
    {
        return condition ? action(builder) : builder;
    }
}
