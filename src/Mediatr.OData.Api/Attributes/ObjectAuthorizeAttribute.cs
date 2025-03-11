using Microsoft.AspNetCore.Authorization;

namespace Mediatr.OData.Api.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ObjectAuthorizeAttribute : AuthorizeAttribute
{
    public string[] Scopes { get; set; } = [];

    //Need to do something withn Scopes / Roles and Policies
}
