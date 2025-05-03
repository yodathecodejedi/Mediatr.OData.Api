using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.Http;

namespace Mediatr.OData.Api.Extensions;

public static class HttpResultExtensions
{
    public static IResult ToODataResults<T>(this IMediatrResult<T> result)
    {
        if (result.IsNotSuccess())
        {
            return result.ToResults();
        }
        return new ODataResults<T> { Data = result.Data };
    }

    public static IResult ToResults(this IMediatrResult result)
    {
        if (result.CustomResult is not null && result.CustomResult is IResult customResult)
        {
            return customResult;
        }

        var statusCode = (int)result.HttpStatusCode;
        var responseMap = new Dictionary<int, Func<object?, string?, IResult>>
        {
            { StatusCodes.Status200OK, (data, _) => Results.Ok(data) },
            { StatusCodes.Status201Created, (data, _) => Results.Created("", data) },
            { StatusCodes.Status204NoContent, (_, _) => Results.NoContent() },
            { StatusCodes.Status400BadRequest, (_, error) => Results.BadRequest(new { Error = error }) },
            { StatusCodes.Status401Unauthorized, (_, error) => Results.Unauthorized() },
            { StatusCodes.Status403Forbidden, (_, error) => Results.Forbid() },
            { StatusCodes.Status404NotFound, (_, error) => Results.NotFound(new { Error = error }) },
            { StatusCodes.Status406NotAcceptable, (_, error) => Results.Problem(error, statusCode: statusCode) },
            { StatusCodes.Status500InternalServerError, (_, error) => Results.Problem(error) },
        };

        var boolTest = responseMap.TryGetValue(statusCode, out Func<object?, string?, IResult>? value2);

        if (!responseMap.TryGetValue(statusCode, out Func<object?, string?, IResult>? value))
        {

            return Results.StatusCode(statusCode);
        }
        var responseResult = value(result.Data, result.Message);
        return responseResult;
    }
}
