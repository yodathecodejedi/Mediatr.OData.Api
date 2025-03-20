using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using System.Net;

namespace Mediatr.OData.Api.Models;

public class ODataQueryOptionsWithPageSize<TDomainObject>(
    ODataQueryContext context,
    HttpRequest request
) : ODataQueryOptions<TDomainObject>(context, request) where TDomainObject : class, IDomainObject
{
    public int PageSize { get; set; } = request.GetPageSizeFromQueryString();
    public bool CountOnly { get; set; } = request.Path.HasValue && request.Path.Value.EndsWith("/$count");

    public Result<dynamic> ApplyODataOptions(IQueryable<TDomainObject> data)
    {
        try
        {

            if (CountOnly)
            {
                return new Result<dynamic>
                {

                    Data = data.Count(),
                    IsSuccess = true,
                    HttpStatusCode = HttpStatusCode.OK
                };
            }
            ODataQuerySettings settings = new() { PageSize = PageSize };
            Result<dynamic> result = new()
            {
                Data = base.ApplyTo(data, settings),
                IsSuccess = true,
                HttpStatusCode = HttpStatusCode.OK
            };
            return result;
        }
        catch (Exception ex)
        {
            //We need to expand this to handle the exception with more context maybe to give the correct statuscode
            return new Result<dynamic>
            {
                Data = null,
                IsSuccess = false,
                Message = ex.Message,
                HttpStatusCode = HttpStatusCode.NotAcceptable
            };
        }
    }

    public Result<dynamic> ApplyODataOptions(object entity)
    {
        try
        {
            ODataQuerySettings settings = new() { PageSize = PageSize };
            Result<dynamic> result = new()
            {
                Data = base.ApplyTo(entity, settings),
                IsSuccess = true,
                HttpStatusCode = HttpStatusCode.OK
            };
            return result;
        }
        catch (Exception ex)
        {
            //We need to expand this to handle the exception with more context maybe to give the correct statuscode
            return new Result<dynamic>
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex,
                HttpStatusCode = HttpStatusCode.NotAcceptable
            };
        }
    }
}