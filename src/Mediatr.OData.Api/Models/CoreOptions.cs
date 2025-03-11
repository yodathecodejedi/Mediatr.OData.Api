using Mediatr.OData.Api.Factories;
using Microsoft.AspNetCore.OData;

namespace Mediatr.OData.Api.Models;

public class CoreOptions
{
    public Action<ODataOptions> ODataOptions { get; set; } = (options) => options.EnableQueryFeatures();
    public MetadataContainerFactory MetadataContainerFactory { get; set; } = new MetadataContainerFactory();
}
