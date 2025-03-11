namespace Mediatr.OData.Api.Enumerations;

[Flags]
public enum HttpOperation
{
    None = 0,
    Post = 1 << 0,
    Put = 1 << 1,
    Patch = 1 << 2,
}
