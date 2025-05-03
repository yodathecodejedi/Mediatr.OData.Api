using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;

namespace Mediatr.OData.Api.Abstractions.Models;

public sealed class DomainObject : IDomainObject<string>
{
    [ODataType]
    public string Type { get; set; } = string.Empty;
    [ODataKey]
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
