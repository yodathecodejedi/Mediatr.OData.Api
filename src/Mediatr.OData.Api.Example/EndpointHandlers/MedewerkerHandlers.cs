using Dapper;
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Example.DomainObjects;
using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
using System.Data;

namespace Mediatr.OData.Api.Example.EndpointHandlers;

[EndpointAuthorize]
public class MedewerkerHandlers
{
    public static string Query(bool withKey = false)
    {
        string sql = @"
                SELECT
	                m.Id,
	                m.Name,
                    m.Description,
	                m.AfdelingId,
	                a.Sleutel AS AfdelingId,
                    a.Sleutel,
                    a.Name,
                    a.Description
                FROM Medewerkers m
                INNER  JOIN Afdeling a
                ON m.AfdelingId = a.Sleutel
                ";
        if (withKey)
        {
            sql += "WHERE m.Id = @key";
        }

        return sql;
    }

    [Endpoint<Medewerker>("medewerkers", EndpointMethod.Get)]
    public class QueryHandler(IDbConnection connection) : IEndpointGetHandler<Medewerker>
    {


        public async Task<Result<dynamic>> Handle(ODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
        {
            Result<dynamic> x = new();
            await Task.CompletedTask;
            try
            {
                connection.Open();

                var medewerkers = await connection.QueryAsync<Medewerker, Afdeling, Medewerker>(
                    Query(),
                    (medewerker, afdeling) =>
                    {
                        medewerker.Afdeling = afdeling;
                        return medewerker;
                    },
                    splitOn: "AfdelingId"
                );

                var data = medewerkers.AsQueryable();

                //Haal mijn data op, op de manier zoals wij dat fijn vinden
                ODataQuerySettings settings = new()
                {
                    PageSize = options.PageSize

                };

                var result = options.ApplyTo(data, settings);


                x.Data = result;
                x.IsSuccess = true;
                x.HttpStatusCode = System.Net.HttpStatusCode.OK;

            }
            catch (ODataException ex)
            {
                x = new()
                {
                    IsSuccess = false,
                    Exception = ex,
                    Message = ex.Message,
                    CustomResult = @"{ ""test"":""Deze functie is fout gegaan!""}",
                    HttpStatusCode = System.Net.HttpStatusCode.NotAcceptable
                };

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return x;
        }
    }

    [Endpoint<Medewerker, int>("medewerkers", EndpointMethod.Get)]
    public class GetByKeyHandler(IDbConnection connection) : IEndpointGetByKeyHandler<Medewerker, int>
    {
        public async Task<Result<dynamic>> Handle(int key, ODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
        {
            Result<dynamic> x = new();
            await Task.CompletedTask;
            try
            {
                connection.Open();

                var medewerkers = await connection.QueryAsync<Medewerker, Afdeling, Medewerker>(
                    Query(true),
                    (medewerker, afdeling) =>
                    {
                        medewerker.Afdeling = afdeling;
                        return medewerker;
                    },
                    new { key },
                    splitOn: "AfdelingId"
                );

                var data = medewerkers.FirstOrDefault();

                //Haal mijn data op, op de manier zoals wij dat fijn vinden
                ODataQuerySettings settings = new()
                {
                    PageSize = options.PageSize,

                };

                var result = options.ApplyTo(data, settings);

                x.Data = result;
                x.IsSuccess = true;
                x.HttpStatusCode = System.Net.HttpStatusCode.OK;

            }
            catch (ODataException ex)
            {
                x = new()
                {
                    IsSuccess = false,
                    Exception = ex,
                    Message = ex.Message,
                    CustomResult = @"{ ""test"":""Deze functie is fout gegaan!""}",
                    //x.Failed("Deze functie is fout gegaan2!");
                    HttpStatusCode = System.Net.HttpStatusCode.NotAcceptable
                };

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return x;
        }
    }
}
