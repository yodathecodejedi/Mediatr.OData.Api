using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Abstractions.Models;

namespace Mediatr.OData.Example.DomainModel.Company
{
    public sealed class Department : IDomainObject<Guid>
    {
        //Keys
        [ODataIgnore]
        public int Id { get; set; }

        [ODataKey]
        public Guid Key { get; set; }

        //Fields
        [ODataDisplayName]
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;

        [ODataIgnore]
        public int? CompanyId { get; set; } = default!;

        //System fields
        public byte[] Hash { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        public DateTime ModifiedAt { get; set; } = default!;

        //Navigation
        public Company? Company { get; set; } = default!;
        public ICollection<Employee>? Employees { get; set; } = default!;
        public ICollection<DomainObject> Members { get; set; } = default!;
    }
}
