namespace Mediatr.OData.Api.Models;

public class TypeDefinition
{
    public string Root { get; set; } = default!;
    public string FirstSegment { get; set; } = default!;
    public bool UseFirstSegment { get; set; } = true;
}
