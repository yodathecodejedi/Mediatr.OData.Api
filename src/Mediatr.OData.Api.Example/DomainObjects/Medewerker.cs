using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Interfaces;

namespace Mediatr.OData.Api.Example.DomainObjects;

public class Medewerker : IDomainObject<int>
{

    public int Id { get; set; } = default!;
    [PropertyHash]
    public string Hash { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Afdeling? Afdeling { get; set; } = default!;
}
