namespace Mediatr.OData.Api.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class ODataDisplayNameAttribute : Attribute { }
