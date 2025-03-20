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
                options.WithClientButton(true)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithDownloadButton(true)
                .WithTitle($"Graph Explorer | {configuration.Title}");
                options.ShowSidebar = true;
            }).RequireAuthorization();
        }
        else
        {
            app.MapScalarApiReference("", options =>
            {
                options.WithClientButton(true)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithDownloadButton(true)
                .WithTitle($"Graph Explorer | {configuration.Title}");
                options.ShowSidebar = true;
            });
        }

        if (configuration.UseHttpsRedirection)
            app.UseHttpsRedirection();
    }
}
