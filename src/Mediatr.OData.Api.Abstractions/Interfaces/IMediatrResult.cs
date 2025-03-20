using System.Net;

namespace Mediatr.OData.Api.Abstractions.Interfaces
{
    public interface IMediatrResult
    {
        bool IsSuccess { get; set; }
        string? Message { get; set; }
        Exception? Exception { get; set; }
        HttpStatusCode HttpStatusCode { get; set; }
        object? Data { get; set; }
        object? CustomResult { get; set; }
    }

    public interface IMediatrResult<T> : IMediatrResult
    {
        new T? Data { get; set; }
    }
}
