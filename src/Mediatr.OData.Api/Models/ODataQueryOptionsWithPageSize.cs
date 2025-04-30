using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using System.Net;

namespace Mediatr.OData.Api.Models;
//TODO => COunt dynamic object with Fieldname Count and Value int
//In applyto this is a Value Object Let's see what comes from this for now we will set it to obect ?
public class ODataQueryOptionsWithPageSize<TDomainObject>(
    ODataQueryContext context,
    HttpRequest request
) : ODataQueryOptions<TDomainObject>(context, request), IODataQueryOptionsWithPageSize<TDomainObject> where TDomainObject : class, IDomainObject
{
    public int PageSize { get; set; } = request.GetPageSizeFromQueryString();
    public bool CountOnly { get; set; } = request.Path.HasValue && request.Path.Value.EndsWith("/$count");

    public IMediatrResult<dynamic> ApplyODataOptions(IQueryable<TDomainObject> data)
    {
        try
        {

            if (CountOnly)
            {
                return new MediatrResult<dynamic>
                {

                    Data = data.Count(),
                    IsSuccess = true,
                    HttpStatusCode = HttpStatusCode.OK
                };
            }

            ODataQuerySettings settings = new() { PageSize = PageSize };
            return new MediatrResult<dynamic>
            {
                Data = base.ApplyTo(data, settings),
                IsSuccess = true,
                HttpStatusCode = HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            //We need to expand this to handle the exception with more context maybe to give the correct statuscode
            return new MediatrResult<dynamic>
            {
                Data = null,
                IsSuccess = false,
                Message = ex.Message,
                HttpStatusCode = HttpStatusCode.NotAcceptable
            };
        }
    }

    public IMediatrResult<dynamic> ApplyODataOptions(object entity)
    {
        try
        {
            ODataQuerySettings settings = new() { PageSize = PageSize };
            return new MediatrResult<dynamic>
            {
                Data = base.ApplyTo(entity, settings),
                IsSuccess = true,
                HttpStatusCode = HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            //We need to expand this to handle the exception with more context maybe to give the correct statuscode
            return new MediatrResult<dynamic>
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex,
                HttpStatusCode = HttpStatusCode.NotAcceptable
            };
        }
    }
}