using Microsoft.AspNetCore.Builder;

namespace Mediatr.OData.Api.Interfaces;

public interface IHttpRequestHandler
{
    Task MapRoutes(WebApplication webApplication);
}