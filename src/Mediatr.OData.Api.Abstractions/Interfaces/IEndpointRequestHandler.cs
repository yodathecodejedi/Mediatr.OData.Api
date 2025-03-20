namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IEndpointRequestHandler<TRequest>
    where TRequest : class
{
    Task<IODataResult<dynamic>> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TDomainObject>
    where TRequest : class
    where TDomainObject : class, IDomainObject
{
    Task<IODataResult<dynamic>> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TDomainObject, TKey>
    where TRequest : class
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<IODataResult<dynamic>> Handle(TKey key, TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TResponse, TDomainObject, TKey>
    where TRequest : class
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<IODataResult<dynamic>> Handle(TKey key, TRequest request, CancellationToken cancellationToken);
}
