namespace Mediatr.OData.Api.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class ODataNavigationAttribute : Attribute
{
}
