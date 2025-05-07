using Mediatr.OData.Api.Abstractions.Models;
using Microsoft.AspNetCore.OData.Formatter;

namespace Mediatr.OData.Api.Extensions;

public static class ResourceContextExtensions
{

    public static bool TryGetODataTypeName(this ResourceContext resourceContext, out string oDataTypeName)
    {
        //Patterns => {root}.{optionalFirstSegment}.{typeName}
        ArgumentNullException.ThrowIfNull(resourceContext, nameof(resourceContext));
        oDataTypeName = default!;

        if (resourceContext.ResourceInstance is DomainObject)
        {
            return false;
        }

        var resourceInstanceType = resourceContext.ResourceInstance.GetType();

        return resourceInstanceType.TryGetODataTypeName(out oDataTypeName);
    }
}
