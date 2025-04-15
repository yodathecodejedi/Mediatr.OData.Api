using Microsoft.AspNetCore.Authorization;

namespace Mediatr.OData.Api.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
public sealed class ODataAuthorizeAttribute : AuthorizeAttribute
{
    public string[] Scopes { get; set; } = [];

    //Need to do something withn Scopes / Roles and Policies
}