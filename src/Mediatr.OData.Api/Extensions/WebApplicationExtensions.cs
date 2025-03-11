using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;

namespace Mediatr.OData.Api.Extensions;

public static class WebApplicationExtensions
{
    public static void AttachODataRoutes(this WebApplication? app, string ConfigurationSection = default!)
    {
        if (app is null) return;

        //This is our OData
        app.MapODataRoutes();

        if (ConfigurationSection.IsNullOrWhiteSpace())
            ConfigurationSection = Constants.OData.DefaultConfigurationSection;

        var configuration = app.Configuration.GetSection(ConfigurationSection).Get<ODataConfiguration>() ?? new ODataConfiguration();

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
