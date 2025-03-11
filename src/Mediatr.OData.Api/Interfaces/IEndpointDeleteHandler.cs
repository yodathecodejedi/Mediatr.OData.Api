using Mediatr.OData.Api.Models;

namespace Mediatr.OData.Api.Interfaces;

public interface IEndpointDeleteHandler<TDomainObject, TKey>
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<Result<dynamic>> Handle(TKey key, CancellationToken cancellationToken);
}