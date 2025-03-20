namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IEndpointDeleteHandler<TDomainObject, TKey>
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<IODataResult<dynamic>> Handle(TKey key, CancellationToken cancellationToken);
}