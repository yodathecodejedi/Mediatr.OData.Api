using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;

namespace Mediatr.OData.Example.DomainModel.Company
{
    public sealed class Employee : IDomainObject<int>
    {
        //Fields
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public Function Function { get; set; } = default!;

        [PropertyInternal]
        public int? DepartmentId { get; set; } = default;

        //Navigation
        public Department? Department { get; set; } = default!;
    }
}
