﻿using Mediatr.OData.Api.Models;

namespace Mediatr.OData.Api.Extensions;

public static class ResultsExtensions
{
    public static Result Success(this object _)
        => new()
        { IsSuccess = true };

    public static Result<T> Success<T>(this T? data)
        => new()
        { IsSuccess = true, Data = data, HttpStatusCode = System.Net.HttpStatusCode.OK };

    public static Result<T> Created<T>(this T data)
        => new()
        { IsSuccess = true, Data = data, HttpStatusCode = System.Net.HttpStatusCode.Created };

    public static Result<T> Failed<T>(this T? data, string message)
        => new()
        { IsSuccess = false, Data = data, Message = message, HttpStatusCode = System.Net.HttpStatusCode.BadRequest };

    public static Result Failed(this object _, string message)
        => new()
        { IsSuccess = false, Message = message, HttpStatusCode = System.Net.HttpStatusCode.BadRequest };

    public static Result<T> Notfound<T>(this T? _, string? message = null)
        => new()
        { IsSuccess = false, Message = message, HttpStatusCode = System.Net.HttpStatusCode.NotFound };

    public static bool IsNotSuccess(this Result result)
        => !result.IsSuccess;
}
