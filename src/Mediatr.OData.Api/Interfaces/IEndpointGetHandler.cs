using Mediatr.OData.Api.Models;

namespace Mediatr.OData.Api.Interfaces;

public interface IEndpointGetHandler<TDomainObject>
    where TDomainObject : class, IDomainObject
{
    Task<Result<dynamic>> Handle(ODataQueryOptionsWithPageSize<TDomainObject> options
        , CancellationToken cancellationToken);
}

public interface IEndpointGetByKeyHandler<TDomainObject, TKey>
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<Result<dynamic>> Handle(TKey key, ODataQueryOptionsWithPageSize<TDomainObject> options
        , CancellationToken cancellationToken);

}

public interface IEndpoinGetByNavigationHandler<TDomainObject, TKey, TNavigationObject>
    where TDomainObject : class, IDomainObject
    where TKey : notnull
    where TNavigationObject : class, IDomainObject
{
    Task<Result<dynamic>> Handle(TKey key, Type TDomainObject, ODataQueryOptionsWithPageSize<TNavigationObject> options
        , CancellationToken cancellationToken);
}