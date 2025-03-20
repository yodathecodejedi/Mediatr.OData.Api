﻿using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json;
using System.Web;

namespace Mediatr.OData.Api.Extensions;

public static class HttpRequestExtensions
{
    public static int GetPageSizeFromQueryString(this HttpRequest request)
    {
        var oDataConfiguration = AppContext.GetData("ODataConfiguration") as ODataConfiguration ?? new ODataConfiguration();
        if (request.QueryString.HasValue is false)
            return oDataConfiguration.PageSize;

        string? pagesizeFromQueryString = HttpUtility.ParseQueryString(request.QueryString.Value)["pagesize"];
        if (pagesizeFromQueryString is null)
            return oDataConfiguration.PageSize;

        if (int.TryParse(pagesizeFromQueryString, out int pageSize))
            return pageSize <= oDataConfiguration.PageSize ? pageSize : oDataConfiguration.PageSize;

        return oDataConfiguration.PageSize;
    }

    public static async Task<T> ParseRequest<T>(this IHttpRequestHandler _, HttpRequest request)
    {
        var jsonOption = request.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;

        if (request.Method == HttpMethod.Get.Method)
        {
            if (!request.QueryString.HasValue)
            {
                return default!;
            }

            var dict = HttpUtility.ParseQueryString(request.QueryString.Value);
            string json = JsonSerializer.Serialize(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
            var model = JsonSerializer.Deserialize<T>(json, jsonOption.JsonSerializerOptions);
            return model!;
        }

        if (!request.Body.CanRead)
        {
            throw new InvalidOperationException("Request body is not readable");
        }

        using var reader = new StreamReader(request.Body);
        var bodyAsString = await reader.ReadToEndAsync();
        using var document = JsonDocument.Parse(bodyAsString);
        var root = document.RootElement;

        var jsonObject = root.EnumerateObject();
        var isBodyWrapper = jsonObject.Count() == 1
            && jsonObject.Any(x => x.Name.Equals("body", StringComparison.CurrentCultureIgnoreCase));
        if (isBodyWrapper)
        {
            var rootProperty = jsonObject.First();
            return BindJsonElement<T>(request, rootProperty.Value, jsonOption);
        }

        return BindJsonElement<T>(request, root, jsonOption);
    }

    private static T BindJsonElement<T>(HttpRequest request, JsonElement jsonElment, JsonOptions jsonOptions)
    {
        var result = jsonElment.Deserialize<T>(jsonOptions.JsonSerializerOptions) ?? throw new InvalidOperationException("Failed to deserialize request body");

        var routeData = request.RouteValues;
        var routeBoundProperties = typeof(T).GetProperties()
            .Where(property => property.GetCustomAttribute<FromRouteAttribute>() is not null);

        foreach (var routeBoundProperty in routeBoundProperties)
        {
            var routeAttribute = routeBoundProperty.GetCustomAttribute<FromRouteAttribute>();
            var routeKey = routeAttribute?.Name ?? routeBoundProperty.Name;

            if (routeData.TryGetValue(routeKey, out var routeValue))
            {
                var convertedRouteValue = Convert.ChangeType(routeValue, routeBoundProperty.PropertyType);
                routeBoundProperty.SetValue(result, convertedRouteValue);
            }
        }

        return result;
    }
}
