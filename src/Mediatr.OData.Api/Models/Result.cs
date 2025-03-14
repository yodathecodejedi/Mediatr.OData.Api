using System.Net;

namespace Mediatr.OData.Api.Models;

public class Result
{
    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public Exception? Exception { get; set; }

    public HttpStatusCode HttpStatusCode { get; set; }

    public object? Data { get; set; }

    /// <summary>
    /// Return directly the custom result if it is not null.
    /// </summary>
    public object? CustomResult { get; set; }

    public static async Task<Result<dynamic>> CreateProblem(HttpStatusCode statusCode, string message, Exception? exception = null)
    {
        await Task.CompletedTask;

        if (statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.Accepted)
            throw new ArgumentException("Status code cannot be OK, Created or Accepted", nameof(statusCode));

        return new Result<dynamic>
        {
            IsSuccess = false,
            HttpStatusCode = statusCode,
            Message = message,
            Exception = exception
        };
    }

    public static async Task<Result<dynamic>> CreateSuccess(object? data, HttpStatusCode statusCode)
    {
        await Task.CompletedTask;

        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.Created && statusCode != HttpStatusCode.Accepted)
            throw new ArgumentException("Status code must be OK, Created or Accepted", nameof(statusCode));

        return new Result<dynamic>
        {
            IsSuccess = true,
            HttpStatusCode = statusCode,
            Data = data
        };
    }
}

public class Result<T> : Result
{
    public new T? Data
    {
        get => (T?)base.Data;
        set => base.Data = value;
    }
}