namespace Mediatr.OData.Api.Abstractions.Interfaces
{
    public interface IODataQueryOptionsWithPageSize<TDomainObject> where TDomainObject : class, IDomainObject
    {
        int PageSize { get; set; }
        bool CountOnly { get; set; }

        IODataResult<dynamic> ApplyODataOptions(IQueryable<TDomainObject> data);
        IODataResult<dynamic> ApplyODataOptions(object entity);
    }
}
