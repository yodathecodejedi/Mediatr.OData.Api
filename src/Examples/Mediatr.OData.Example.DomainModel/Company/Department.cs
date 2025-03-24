using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;

namespace Mediatr.OData.Example.DomainModel.Company
{
    public sealed class Department : IDomainObject<int>
    {
        //Fields
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;

        [PropertyInternal]
        public int? CompanyId { get; set; } = default!;

        //Navigation
        public Company? Company { get; set; } = default!;

        public ICollection<Employee>? Employees { get; set; } = default!;

    }
}
