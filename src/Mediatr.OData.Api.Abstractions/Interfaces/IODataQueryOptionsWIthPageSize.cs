namespace Mediatr.OData.Api.Abstractions.Interfaces;
public interface IODataQueryOptionsWithPageSize<TDomainObject> where TDomainObject : class, IDomainObject
{
    int PageSize { get; set; }
    bool CountOnly { get; set; }

    IMediatrResult<dynamic> ApplyODataOptions(IQueryable<TDomainObject> data);
    IMediatrResult<dynamic> ApplyODataOptions(object entity);
}
