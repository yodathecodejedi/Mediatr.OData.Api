using Microsoft.AspNetCore.OData.Deltas;

namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IEndpointPutHandler<TDomainObject, TKey> where TDomainObject : class, IDomainObject<TKey>
{
    Task<IODataResult<dynamic>> Handle(TKey key, Delta<TDomainObject> domainObjectDelta, IODataQueryOptionsWithPageSize<TDomainObject> options, CancellationToken cancellationToken);
}
