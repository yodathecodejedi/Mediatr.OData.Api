using Mediatr.OData.Api.Enumerations;

namespace Mediatr.OData.Api.Models;

internal record EndpointKey
{
    public required EndpointMethod HttpMethod { set; get; } = EndpointMethod.Get;
    public required string Route { set; get; } = string.Empty;
    public string RouteSegment { set; get; } = string.Empty;
    public required bool KeyInRoute { set; get; } = false;
    public required Type KeyType { set; get; } = default!;
    public required Type ObjectType { set; get; } = default!;

}
