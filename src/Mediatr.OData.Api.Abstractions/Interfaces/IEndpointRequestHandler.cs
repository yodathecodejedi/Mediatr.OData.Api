namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IEndpointRequestHandler<TRequest>
    where TRequest : class
{
    Task<IMediatrResult<dynamic>> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TDomainObject>
    where TRequest : class
    where TDomainObject : class, IDomainObject
{
    Task<IMediatrResult<dynamic>> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TDomainObject, TKey>
    where TRequest : class
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<IMediatrResult<dynamic>> Handle(TKey key, TRequest request, CancellationToken cancellationToken);
}

public interface IEndpointRequestHandler<TRequest, TResponse, TDomainObject, TKey>
    where TRequest : class
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<IMediatrResult<dynamic>> Handle(TKey key, TRequest request, CancellationToken cancellationToken);
}
