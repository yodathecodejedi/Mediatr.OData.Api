using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Models;
using System.Net;

namespace Mediatr.OData.Api.Factories
{
    public class MediatrResultFactory
    {
        public static async Task<IMediatrResult<dynamic>> CreateProblem(HttpStatusCode statusCode, string message, Exception? exception = null)
        {
            await Task.CompletedTask;

            if (statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.Accepted)
                throw new ArgumentException("Status code cannot be OK, Created or Accepted", nameof(statusCode));

            return new MediatrResult<dynamic>
            {
                IsSuccess = false,
                HttpStatusCode = statusCode,
                Message = message,
                Exception = exception
            };
        }

        public static async Task<IMediatrResult<dynamic>> CreateSuccess(object? data, HttpStatusCode statusCode)
        {
            await Task.CompletedTask;

            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.Created && statusCode != HttpStatusCode.Accepted)
                throw new ArgumentException("Status code must be OK, Created or Accepted", nameof(statusCode));

            return new MediatrResult<dynamic>
            {
                IsSuccess = true,
                HttpStatusCode = statusCode,
                Data = data
            };
        }
    }
}
