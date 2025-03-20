using Mediatr.OData.Api.Abstractions.Interfaces;
using System.Net;

namespace Mediatr.OData.Api.Models;

public class MediatrResult : IMediatrResult
{
    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public Exception? Exception { get; set; }

    public HttpStatusCode HttpStatusCode { get; set; }

    public object? Data { get; set; }

    public object? CustomResult { get; set; }
}

public class MediatrResult<T> : MediatrResult, IMediatrResult<T>
{
    public new T? Data
    {
        get => (T?)base.Data;
        set => base.Data = value;
    }
}