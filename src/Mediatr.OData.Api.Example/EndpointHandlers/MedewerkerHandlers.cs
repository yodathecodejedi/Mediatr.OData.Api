using Dapper;
using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Example.DomainObjects;
using System.Data;

namespace Mediatr.OData.Api.Example.EndpointHandlers;

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


        public async Task<IMediatrResult<dynamic>> Handle(IODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
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

            return options.ApplyODataOptions(data);
        }
    }

    [Endpoint<Medewerker, int>("medewerkers", EndpointMethod.Get)]
    public class GetByKeyHandler(IDbConnection connection) : IEndpointGetByKeyHandler<Medewerker, int>
    {
        public async Task<IMediatrResult<dynamic>> Handle(int key, IODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
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

            var data = medewerkers.FirstOrDefault() ?? default!;

            return options.ApplyODataOptions(data);
        }
    }
}
