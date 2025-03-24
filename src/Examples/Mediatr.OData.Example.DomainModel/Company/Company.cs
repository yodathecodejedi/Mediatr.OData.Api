using Mediatr.OData.Api.Abstractions.Interfaces;

namespace Mediatr.OData.Example.DomainModel.Company
{
    public sealed class Company : IDomainObject<int>
    {
        //Fields 
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;

        //Relation navigation for OData
        public ICollection<Department>? Departments { get; set; } = default!;
    }
}
