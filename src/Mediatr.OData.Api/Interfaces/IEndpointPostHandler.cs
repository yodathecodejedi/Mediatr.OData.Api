using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Deltas;

namespace Mediatr.OData.Api.Interfaces;

public interface IEndpointPostHandler<TDomainObject>
    where TDomainObject : class, IDomainObject
{
    Task<Result<dynamic>> Handle(Delta<TDomainObject> domainObjectDelta, ODataQueryOptionsWithPageSize<TDomainObject> options, CancellationToken cancellationToken);
}

