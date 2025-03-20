using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Models;

namespace Mediatr.OData.Api.Extensions;

public static class MediatrResultExtensions
{
    public static IMediatrResult Success(this object _)
        => new MediatrResult
        { IsSuccess = true };

    public static IMediatrResult<T> Success<T>(this T? data)
        => new MediatrResult<T>
        { IsSuccess = true, Data = data, HttpStatusCode = System.Net.HttpStatusCode.OK };

    public static IMediatrResult<T> Created<T>(this T data)
        => new MediatrResult<T>
        { IsSuccess = true, Data = data, HttpStatusCode = System.Net.HttpStatusCode.Created };

    public static IMediatrResult<T> Failed<T>(this T? data, string message)
        => new MediatrResult<T>
        { IsSuccess = false, Data = data, Message = message, HttpStatusCode = System.Net.HttpStatusCode.BadRequest };

    public static IMediatrResult Failed(this object _, string message)
        => new MediatrResult
        { IsSuccess = false, Message = message, HttpStatusCode = System.Net.HttpStatusCode.BadRequest };

    public static IMediatrResult<T> Notfound<T>(this T? _, string? message = null)
        => new MediatrResult<T>
        { IsSuccess = false, Message = message, HttpStatusCode = System.Net.HttpStatusCode.NotFound };

    public static bool IsNotSuccess(this IMediatrResult result)
        => !result.IsSuccess;
}
