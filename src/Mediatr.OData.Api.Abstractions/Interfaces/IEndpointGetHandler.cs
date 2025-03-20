namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IEndpointGetHandler<TDomainObject>
    where TDomainObject : class, IDomainObject
{
    Task<IODataResult<dynamic>> Handle(IODataQueryOptionsWithPageSize<TDomainObject> options
        , CancellationToken cancellationToken);
}

public interface IEndpointGetByKeyHandler<TDomainObject, TKey>
    where TDomainObject : class, IDomainObject<TKey>
{
    Task<IODataResult<dynamic>> Handle(TKey key, IODataQueryOptionsWithPageSize<TDomainObject> options
        , CancellationToken cancellationToken);

}

public interface IEndpoinGetByNavigationHandler<TDomainObject, TKey, TNavigationObject>
    where TDomainObject : class, IDomainObject
    where TKey : notnull
    where TNavigationObject : class, IDomainObject
{
    Task<IODataResult<dynamic>> Handle(TKey key, Type TDomainObject, IODataQueryOptionsWithPageSize<TNavigationObject> options
        , CancellationToken cancellationToken);
}