namespace Mediatr.OData.Api.Models;

public class ODataConfiguration
{
    public string Title { get; set; } = Constants.OData.DefaultTitle;
    public TypeDefinition TypeDefinition { get; set; } = new();
    public bool SecureAPI { get; set; } = false;
    public bool SecureWebInterface { get; set; } = false;
    public string RoutePrefix { get; set; } = Constants.OData.DefaultRoutePrefix;
    public int PageSize { get; set; } = Constants.OData.DefaultPageSize;
    public bool UseHttpsRedirection { get; set; } = true;
}

public class TypeDefinition
{
    public string Root { get; set; } = default!;
    public string FirstSegment { get; set; } = default!;
}
