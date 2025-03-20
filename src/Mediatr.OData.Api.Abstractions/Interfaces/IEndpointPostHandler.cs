using Microsoft.AspNetCore.OData.Deltas;

namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IEndpointPostHandler<TDomainObject>
    where TDomainObject : class, IDomainObject
{
    Task<IODataResult<dynamic>> Handle(Delta<TDomainObject> domainObjectDelta, IODataQueryOptionsWithPageSize<TDomainObject> options, CancellationToken cancellationToken);
}

