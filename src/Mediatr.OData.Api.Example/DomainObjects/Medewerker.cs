using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;

namespace Mediatr.OData.Api.Example.DomainObjects;

public class Medewerker : IDomainObject<int>
{

    public int Id { get; set; } = default!;
    [PropertyHash]
    public string Hash { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;
    [PropertyInternal]
    public int AfdelingId { get; set; }

    public Afdeling? Afdeling { get; set; } = default!;
}
