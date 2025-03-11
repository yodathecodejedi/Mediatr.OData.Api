using Mediatr.OData.Api.Extensions;
using Microsoft.OData;

namespace Mediatr.OData.Api.Serializers;

public class SerializerResult(ODataResource resource, IEnumerable<ODataPropertyInfo> properties)
{
    public List<ODataPropertyInfo> Processed { get; set; } = [];
    public IEnumerable<ODataPropertyInfo> Remaining { get; set; } = properties;

    public ODataResource Resource { get; set; } = resource;

    public int Count => Remaining.Count();

    public ODataPropertyInfo TryGetPropertyInfo(string name)
    {
        return Remaining.FirstOrDefault(p => p.Name.Equals(name)) ?? default!;
    }

    public void SetETag(string value)
    {
        Resource.ETag = value;
    }

    public void Add(ODataPropertyInfo propertyInfo)
    {
        //It is a final result and will not be processed again
        Remove(propertyInfo);
        Processed.TryAddProperty(propertyInfo);
    }

    public void Replace(ODataPropertyInfo propertyInfo)
    {
        //We replace it with the new property but will be processed again
        Remove(propertyInfo);
        Remaining = Remaining.Append(propertyInfo);
    }

    public void AddRemaining()
    {
        foreach (var oDataProperty in Remaining)
        {
            Processed.TryAddProperty(oDataProperty);
        }
    }

    public void Remove(ODataPropertyInfo propertyInfo)
    {
        Remaining = Remaining.Where(p => !p.Name.Equals(propertyInfo.Name));
    }

    public void Remove(string name)
    {
        Remaining = Remaining.Where(p => !p.Name.Equals(name));
    }

    public ODataResource Result()
    {
        Resource.Properties = Processed.OrderBy(p => p.Name);
        return Resource;
    }
}
