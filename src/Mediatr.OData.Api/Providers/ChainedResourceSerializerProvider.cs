using Mediatr.OData.Api.Serializers;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Mediatr.OData.Api.Providers;

public class ChainedResourceSerializerProvider : ODataSerializerProvider
{
    private readonly ChainedResourceSerializer _chainedResourceSerializer;

    public ChainedResourceSerializerProvider(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _chainedResourceSerializer = new ChainedResourceSerializer(this);
    }

    public override ODataEdmTypeSerializer? GetEdmTypeSerializer(IEdmTypeReference edmType)
    {
        if (edmType.IsEntity() || edmType.IsComplex())
        {
            return _chainedResourceSerializer;
        }
        return (ODataEdmTypeSerializer)base.GetEdmTypeSerializer(edmType);
    }
}