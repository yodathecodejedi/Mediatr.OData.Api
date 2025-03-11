using Mediatr.OData.Api.Models;

namespace Mediatr.OData.Api.Interfaces;

public interface IEndpointRequestHandler<TRequest>
    where TRequest : class
{
    Task<Result<dynamic>> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TDomainObject>
    where TRequest : class
    where TDomainObject : class, IDomainObject
{
    Task<Result<dynamic>> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TDomainObject, TKey>
    where TRequest : class
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<Result<dynamic>> Handle(TKey key, TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TResponse, TDomainObject, TKey>
    where TRequest : class
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<Result<dynamic>> Handle(TKey key, TRequest request, CancellationToken cancellationToken);
}
