using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Mediatr.OData.Example.DomainModel.Company
{
    public sealed class Department : IDomainObject<Guid>
    {
        //Keys
        [Key]
        public int Id { get; set; }

        [ODataKey]
        public Guid Key { get; set; }

        //Fields
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;

        [Internal]
        public int? CompanyId { get; set; } = default!;

        //System fields
        public byte[] Hash { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        public DateTime ModifiedAt { get; set; } = default!;

        //Navigation
        public Company? Company { get; set; } = default!;
        public ICollection<Employee>? Employees { get; set; } = default!;

    }
}
