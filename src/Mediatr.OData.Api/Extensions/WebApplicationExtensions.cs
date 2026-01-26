using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Scalar.AspNetCore;

namespace Mediatr.OData.Api.Extensions;

public static class WebApplicationExtensions
{
    public static void AttachODataRoutes(this WebApplication? app)
    {
        if (app is null) return;

        app.UseODataRouteDebug(); // Enables route debugging

        //This is our OData
        app.MapODataRoutes();

        var configuration = AppContext.GetData("ODataConfiguration") as ODataConfiguration ?? new ODataConfiguration();

        app.AddMetadataRoute(configuration);

        // Configure the HTTP request pipeline.
        // This is our OData
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "/openapi/{documentName}.json";
        });

        if (configuration.SecureAPI || configuration.SecureWebInterface)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
        if (configuration.SecureWebInterface)
        {
            app.MapScalarApiReference("", options =>
            {
                options
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithDocumentDownloadType(DocumentDownloadType.Both)
                .WithTitle($"Graph Explorer | {configuration.Title}");
                options.ShowSidebar = true;
            }).RequireAuthorization();
        }
        else
        {
            app.MapScalarApiReference("", options =>
            {
                options
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithDocumentDownloadType(DocumentDownloadType.Both)
                .WithTitle($"Graph Explorer | {configuration.Title}");
                options.ShowSidebar = true;
            });
        }

        if (configuration.UseHttpsRedirection)
            app.UseHttpsRedirection();
    }
}
