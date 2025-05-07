using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Abstractions.Models;

namespace Mediatr.OData.Api.Extensions;

public static class DomainObjectExtensions
{
    public static DomainObject ToDomainObject<TDomainObject>(this TDomainObject domainObject) where TDomainObject : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(domainObject);
        var domainObjectType = domainObject.GetType();

        //Get the KeyValue
        var keyFound = domainObjectType.TryGetKeyProperty(out var keyProperty);
        var keyValue = domainObject.GetPropertyValue(keyProperty.Name)?.ToString() ?? default!;
        var displayNameFound = domainObjectType.TryGetDisplayNameProperty(out var displayNameProperty);
        if (!displayNameFound)
            throw new ArgumentException($"DisplayName property not found in {domainObjectType.Name}, please use the ODataDisplayName attribute on the property you want to display for a generic DomainObject.");

        var displayNameValue = domainObject.GetPropertyValue(displayNameProperty.Name)?.ToString() ?? default!;
        var ODataObjectTypeFound = domainObjectType.TryGetODataTypeName(out var oDataTypeName);


        if (keyFound && displayNameFound && ODataObjectTypeFound)
            return new DomainObject
            {
                Type = oDataTypeName,
                DisplayName = displayNameValue,
                Key = keyValue
            };
        else
            return default!;
    }

    public static IQueryable<DomainObject> ToDomainObjects(this IQueryable<IDomainObject>? domainObjects)
    {
        if (domainObjects == null)
            return Enumerable.Empty<DomainObject>().AsQueryable();

        return domainObjects.Select(domainObject => domainObject.ToDomainObject()).AsQueryable();
    }
}
