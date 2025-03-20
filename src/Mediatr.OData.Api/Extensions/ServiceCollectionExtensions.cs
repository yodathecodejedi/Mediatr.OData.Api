using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Factories;
using Mediatr.OData.Api.Models;
using Mediatr.OData.Api.Providers;
using Mediatr.OData.Api.Serializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OData.ModelBuilder;
namespace Mediatr.OData.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddODataEndpoints(this IServiceCollection services, string defaultRoutePrefix = Constants.OData.DefaultRoutePrefix)
    {
        var coreOptions = new CoreOptions();
        defaultRoutePrefix = SanitizeRoutePrefix(defaultRoutePrefix);

        var containers = coreOptions.MetadataContainerFactory
            .CreateContainers(services, defaultRoutePrefix)
            .ToList();

        //input, output formatters
        var outputFormaters = ODataOutputFormatterFactory.Create();
        foreach (var formatter in outputFormaters)
        {
            // Fix for issue where JSON formatter does include charset in the Content-Type header
            if (formatter.SupportedMediaTypes.Contains("application/json")
                && !formatter.SupportedMediaTypes.Contains("application/json; charset=utf-8"))
                formatter.SupportedMediaTypes.Add("application/json; charset=utf-8");
        }
        services.AddSingleton<IEnumerable<ODataOutputFormatter>>(outputFormaters);

        var inputFormatters = JsonInputFormatterFactory.CreateAndAdd();
        services.AddSingleton<IEnumerable<ODataInputFormatter>>(inputFormatters);

        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>());

        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ODataMvcOptionsSetup>());

        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<JsonOptions>, ODataJsonOptionsSetup>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IODataQueryRequestParser, DefaultODataQueryRequestParser>());

        services.TryAddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();
        services.TryAddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();

        services.AddOptions<ODataOptions>().Configure(odataOptions =>
        {
            coreOptions.ODataOptions(odataOptions);

            foreach (var container in containers)
            {
                odataOptions.AddRouteComponents(
                    container.RoutePrefix,
                    container.EdmModel,
                    services =>
                    {
                        services.AddSingleton<ODataSerializerProvider, ChainedResourceSerializerProvider>();
                        services.AddSingleton<ODataResourceSerializer, ChainedResourceSerializer>();
                    }
                );
            }
        });

        return services;
    }

    public static void MapODataRoutes(this WebApplication app)
    {
        var httpRequestHandlers = app.Services.GetServices<IHttpRequestHandler>();
        foreach (var requestHandler in httpRequestHandlers)
        {
            requestHandler.MapRoutes(app);
        }
    }

    private static string SanitizeRoutePrefix(string routePrefix)
    {
        if (routePrefix.Length > 0 && routePrefix[0] != '/' && routePrefix[^1] != '/')
        {
            return routePrefix;
        }

        return routePrefix.Trim('/');
    }
}
