using System.Text.Json.Serialization;

namespace Mediatr.OData.Api.Models;

///TODO: Refactor ODataQueryResults to ODataQueryResults
///TODO: Create ODataQueryResults where IEnumerable<T> will be TDomainObject
///TODO: Change EndpointHandler : Produces where a singluar type is used to use ODataQueryResults<TDomainObject>
public sealed class ODataQueryResults<TDomainObject>
{
    public IEnumerable<TDomainObject> Value { set; get; } = [];

    [JsonPropertyName("@odata.count")]
    public int? TotalCount { set; get; }
}
