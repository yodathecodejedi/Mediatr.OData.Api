using Mediatr.OData.Api.Abstractions.Interfaces;
using System.ComponentModel.DataAnnotations;


namespace Mediatr.OData.Api.Example.DomainObjects;

public class Bedrijf : IDomainObject<int>
{
    [Key]
    public int Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    public ICollection<Afdeling>? AfdelingenCollection { get; set; } = [];
}
