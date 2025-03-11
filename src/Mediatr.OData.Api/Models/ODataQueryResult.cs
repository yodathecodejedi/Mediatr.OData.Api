using System.Text.Json.Serialization;

namespace Mediatr.OData.Api.Models;

public class ODataQueryResult<TDomainObject>
{
    public IEnumerable<TDomainObject> Value { set; get; } = [];

    [JsonPropertyName("@odata.count")]
    public int? TotalCount { set; get; }
}
