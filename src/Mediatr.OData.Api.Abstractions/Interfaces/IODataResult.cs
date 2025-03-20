using System.Net;

namespace Mediatr.OData.Api.Abstractions.Interfaces
{
    public interface IODataResult
    {
        bool IsSuccess { get; set; }
        string? Message { get; set; }
        Exception? Exception { get; set; }
        HttpStatusCode HttpStatusCode { get; set; }
        object? Data { get; set; }
        object? CustomResult { get; set; }
    }

    public interface IODataResult<T> : IODataResult
    {
        new T? Data { get; set; }
    }
}
