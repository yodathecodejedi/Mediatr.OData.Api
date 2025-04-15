using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Interfaces;

namespace Mediatr.OData.Example.DomainModel.Company
{
    public sealed class Company : IDomainObject<int>
    {
        //Keys 
        [ODataIgnore]
        public int Id { get; set; }
        //This should be an implicit key
        public Guid Key { get; set; }
        //Fields 
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;
        //System fields
        [ODataETag] //Could also be implicit
        public byte[] Hash { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        public DateTime ModifiedAt { get; set; } = default!;

        //Relation navigation for OData
        //Could be [ODataNavigation]
        public ICollection<Department>? Departments { get; set; } = default!;
    }
}
