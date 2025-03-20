namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IEndpointDeleteHandler<TDomainObject, TKey>
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<IMediatrResult<dynamic>> Handle(TKey key, CancellationToken cancellationToken);
}