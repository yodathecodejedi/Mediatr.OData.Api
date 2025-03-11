
using Mediatr.OData.Api.Enumerations;

namespace Mediatr.OData.Api.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class PropertyModeAttribute : Attribute
{
    public HttpOperation Operation { get; set; } = HttpOperation.None;
    public Mode Mode { get; set; } = Mode.Allowed;

    public PropertyModeAttribute()
    {

    }

    public PropertyModeAttribute(HttpOperation operation = HttpOperation.None, Mode mode = Mode.Allowed)
    {
        Operation = operation;
        Mode = mode;
    }
}
