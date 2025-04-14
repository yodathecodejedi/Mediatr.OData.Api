using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Models;
using Microsoft.OData;
using System.Reflection;

namespace Mediatr.OData.Api.Serializers;

public class SerializerResult(ODataResource resource, IEnumerable<ODataPropertyInfo> properties, Type clrType)
{
    private readonly ODataConfiguration oDataConfiguration = AppContext.GetData("ODataConfiguration") as ODataConfiguration ?? default!;

    public List<ODataPropertyInfo> Processed { get; set; } = [];
    public IEnumerable<ODataPropertyInfo> Remaining { get; set; } = properties;

    public ODataResource Resource { get; set; } = resource;

    public Type ClrType { get; set; } = clrType;

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
        //Resource.Properties = Processed.OrderBy(p => p.Name);
        Resource.Properties = OrderProperties();
        return Resource;
    }

    private IEnumerable<ODataPropertyInfo> OrderProperties()
    {
        if (!oDataConfiguration.Formatting.OrderPropertiesByModel || ClrType is null)
        {
            return Processed.OrderBy(p => p.Name);
        }

        var propertyOrder = ClrType
          .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
          .OrderBy(p => p.MetadataToken)
          .Select(p => p.Name)
          .ToList();
        return [.. Processed.OrderBy(p =>
                {
                    var index = propertyOrder.IndexOf(p.Name);
                    return index < 0 ? int.MaxValue : index;
                })];
    }
}
