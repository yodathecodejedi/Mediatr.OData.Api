using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Mediatr.OData.Example.DomainModel.Company
{
    public sealed class Employee : IDomainObject<int>
    {
        //Keys
        [Internal]
        public int Id { get; set; }
        [Key]
        public Guid Key { get; set; } = default!;

        //Fields
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public Function? Function { get; set; } = default!;

        //System fields
        [ODataETag]
        public byte[] Hash { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        public DateTime ModifiedAt { get; set; } = default!;

        [Internal]
        public int? DepartmentId { get; set; } = default;

        //Navigation
        public Department? Department { get; set; } = default!;
    }
}
