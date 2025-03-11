using Mediatr.OData.Api.Enumerations;
using System.Reflection;

namespace Mediatr.OData.Api.Models;

public sealed class ValidatedProperty
{
    public string Name { get; set; } = default!;
    public Mode Mode { get; set; } = default!;
    public HttpOperation Operation { get; set; } = default!;

    public bool IsMissing { get; set; } = false;
    public bool IsNullable { get; set; } = false;
    public bool IsEnum { get; set; } = false;
    public bool IsObject { get; set; } = false;
    public bool IsNavigation { get; set; } = false;
    public bool IsKey { get; set; } = false;

    public PropertyInfo Info { get; set; } = default!;
    public Type Type { get; set; } = default!;
    public PropertyCategory Category { get; set; }
    public object? Value { get; set; } = default!;


    public ValidatedProperty()
    {

    }
}