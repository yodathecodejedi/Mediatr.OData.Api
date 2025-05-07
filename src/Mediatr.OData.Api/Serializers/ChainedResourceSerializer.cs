using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;

namespace Mediatr.OData.Api.Serializers;

public class ChainedResourceSerializer : ODataResourceSerializer
{
    private readonly List<ResourceSerializer> _serializers = [];

    public ChainedResourceSerializer(IODataSerializerProvider serializerProvider) : base(serializerProvider)
    {
        _serializers.Add(new ODataGenericTypeResourceSerializer());
        _serializers.Add(new ODataTypeResourceSerializer());
        _serializers.Add(new ODataIgnorePropertiesSerializer());
        _serializers.Add(new ODataETagResourceSerializer());
        _serializers.Add(new DateTimeOffsetResourceSerializer());
        _serializers.Add(new DateResourceSerializer());
        _serializers.Add(new TimeOfDayResourceSerializer());
        _serializers.Add(new EnumResourceSerializer());
    }

    public override ODataResource CreateResource(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        ODataResource oDataResource = base.CreateResource(selectExpandNode, resourceContext);
        List<ODataPropertyInfo> oDataProperties = [.. oDataResource.Properties.Where(p => p is ODataProperty property && property.Value != null)];
        var clrType = resourceContext.ResourceInstance.GetType();
        SerializerResult resourceSerializerResult = new(oDataResource, oDataProperties, clrType);

        foreach (var serializer in _serializers)
        {
            serializer.Process(resourceSerializerResult, selectExpandNode, resourceContext);
        }
        resourceSerializerResult.AddRemaining();

        return resourceSerializerResult.Result();
    }
}
