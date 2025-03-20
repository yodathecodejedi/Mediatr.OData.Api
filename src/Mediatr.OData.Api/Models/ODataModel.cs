using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Reflection;

namespace Mediatr.OData.Api.Models;
public class ODataModel<TBindingModel>
{
    public TBindingModel? Value { get; set; }

    public static async Task<InputFormatterResult?> ProcessAsync(HttpContext context, ParameterInfo parameter)
    {
        var odataFeature = context.Features.Get<IODataFeature>();
        if (odataFeature is null)
            AddODataFeature(context);
        var modelState = new ModelStateDictionary();
        var modelName = parameter.Name!;
        var provider = new EmptyModelMetadataProvider();
        var modelMetadata = provider.GetMetadataForType(typeof(TBindingModel));
        var inputContext = new InputFormatterContext(
            context,
            modelName,
            modelState,
            modelMetadata,
            (stream, encoding) => new StreamReader(stream, encoding)
            );
        InputFormatterResult? inputResult = default;
        var odataInputFormatters = context.RequestServices.GetRequiredService<IEnumerable<ODataInputFormatter>>();
        //Only those that can read the input
        var correctFormatters = odataInputFormatters.Where(ODataInputFormatter => ODataInputFormatter.CanRead(inputContext)).ToList();
        foreach (var inputFormatter in odataInputFormatters)
        {
            inputResult = await inputFormatter.ReadAsync(inputContext);
            if (inputResult is not null && !inputResult.HasError) { break; }
        }
        if (inputResult is null)
            throw new BadHttpRequestException("The information could not be processed, the data is malformatted");
        if (inputResult.HasError)
            throw new BadHttpRequestException("The information could not be processed, the data is malformatted");
        if (!inputResult.IsModelSet)
            throw new BadHttpRequestException("The information could not be processed, the data is malformatted");
        return inputResult;
    }

    private static void AddODataFeature(HttpContext httpContext)
    {
        var container = (httpContext.GetEndpoint()?.Metadata.OfType<ODataMetadataContainer>().SingleOrDefault()) ??
            throw new InvalidOperationException("ODataMetadataContainer not found");

        //var odataOptions = httpContext.RequestServices.GetRequiredService<IOptions<ODataOptions>>().Value;
        var route = httpContext.GetEndpoint()?.Metadata.OfType<EndpointMetadata>().SingleOrDefault()?.Route;

        if (route.IsNullOrWhiteSpace())
            throw new InvalidOperationException("Route not found");

        IEdmNavigationSource edmNavigationSource = container.EdmModel.FindDeclaredNavigationSource(route);

        var edmEntitySet = container.EdmModel.EntityContainer.FindEntitySet(route);
        var entitySetSegment = new EntitySetSegment(edmEntitySet);
        var segments = new List<ODataPathSegment> { entitySetSegment };

        if (httpContext.Request.RouteValues.TryGetValue("key", out var key))
        {
            var entityType = edmNavigationSource.EntityType;
            var keyName = edmNavigationSource.EntityType.DeclaredKey.SingleOrDefault();
            keyName ??= edmNavigationSource.EntityType.Key().Single();

            var keySegment = new KeySegment(
                keys: new Dictionary<string, object> { { keyName.Name, key! } },
                edmType: entityType,
                navigationSource: edmNavigationSource);

            segments.Add(keySegment);
        }

        var path = new ODataPath(segments);
        var feature = new ODataFeature
        {
            Path = path,
            Model = container.EdmModel,
            RoutePrefix = container.RoutePrefix,
        };

        httpContext.Features.Set<IODataFeature>(feature);
    }
}

public class ODataModel<TDomainObject, TBindingModel> : ODataModel<TBindingModel> where TDomainObject : class, IDomainObject
{
    public static async ValueTask<ODataModel<TDomainObject, TBindingModel>?> BindAsync(HttpContext context,
                                                   ParameterInfo parameter)
    {
        var inputResult = await ProcessAsync(context, parameter);

        return new ODataModel<TDomainObject, TBindingModel>
        {
            Value = (TBindingModel)inputResult?.Model!
        };
    }
}

public class ODataModel<TDomainObject, TKey, TBindingModel> : ODataModel<TBindingModel> where TDomainObject : class, IDomainObject<TKey>
{
    public static async ValueTask<ODataModel<TDomainObject, TKey, TBindingModel>?> BindAsync(HttpContext context,
                                                   ParameterInfo parameter)
    {
        var inputResult = await ProcessAsync(context, parameter);
        return new ODataModel<TDomainObject, TKey, TBindingModel>
        {
            Value = (TBindingModel)inputResult?.Model!
        };
    }
}
