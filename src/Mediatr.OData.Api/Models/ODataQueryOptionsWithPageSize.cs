using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Mediatr.OData.Api.Models;

public class ODataQueryOptionsWithPageSize<TDomainObject>(
    IConfiguration configuration,
    ODataQueryContext context,
    HttpRequest request
) : ODataQueryOptions<TDomainObject>(context, request) where TDomainObject : class, IDomainObject
{
    public int PageSize { get; set; } = request.GetPageSizeFromQueryString(configuration);

    public Result<dynamic> ApplyODataOptions(IQueryable query)
    {
        try
        {
            ODataQuerySettings settings = new() { PageSize = PageSize };
            Result<dynamic> result = new()
            {
                Data = base.ApplyTo(query, settings),
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