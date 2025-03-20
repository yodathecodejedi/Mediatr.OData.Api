using Microsoft.AspNetCore.Builder;

namespace Mediatr.OData.Api.Abstractions.Interfaces;

public interface IHttpRequestHandler
{
    Task MapRoutes(WebApplication webApplication);
}