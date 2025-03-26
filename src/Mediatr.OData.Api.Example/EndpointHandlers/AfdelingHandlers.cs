using Dapper;
using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Abstractions.Enumerations;
using Mediatr.OData.Api.Abstractions.Interfaces;
using Mediatr.OData.Api.Example.DomainObjects;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Factories;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Deltas;
using System.Data;
using System.Net;

namespace Mediatr.OData.Api.Example.EndpointHandlers;


[EndpointGroup("afdelingen")]
public class AfdelingHandlers
{
    public static string AfdelingQuery(bool withKey = false)
    {
        //string sql = @"
        //    SELECT
        //        a.Sleutel,
        //        a.Name,
        //        a.Description,
        //        a.Sleutel AS AfdelingId,
        //        m.AfdelingId,                    
        //        m.Id,
        //        m.Name,
        //        m.Description
        //    FROM Medewerkers m
        //    LEFT JOIN Afdeling a
        //    ON m.AfdelingId = a.Sleutel
        //    ";
        string sql = @"
            SELECT 
                a.[Sleutel]
                ,a.[Name]
                ,a.[Description]
                ,a.[BedrijfId]
                ,AfdelingId = a.[Sleutel]
                ,m.AfdelingId
                ,m.AfdelingId
                ,m.Id
                ,m.Description
                ,m.Name
                ,a.BedrijfId
                ,BedrijfId = b.Id
                ,b.Id
                ,b.Name
                ,b.Description
            FROM [Examples].[dbo].[Afdeling] a
            LEFT JOIN [Examples].[dbo].Medewerkers m
            ON m.AfdelingId = a.Sleutel
            LEFT JOIN [Examples].[dbo].Bedrijf b
            ON b.Id = a.BedrijfId
        ";
        if (withKey)
        {
            sql += "WHERE a.Sleutel = @key";
        }

        return sql;
    }

    public static string MedewerkerQuery()
    {
        return @"
            SELECT
                m.AfdelingId,                    
                m.Id,
                m.Name,
                m.Description
            FROM Medewerkers m
            WHERE m.AfdelingId = @key
            ";
    }

    public static string BedrijfQuery()
    {
        return @"
            SELECT
                b.Id,
                b.Name,
                b.Description
            FROM Bedrijf b
            WHERE b.Id = (SELECT TOP 1 BedrijfId FROM dbo.Afdeling WHERE Id = @key)
            ";
    }

    public static async Task<IQueryable<Afdeling>> GetAfdelingenFromDB(IDbConnection connection)
    {
        try
        {
            //connection.Open();
            //var afdelingenDictionary = new Dictionary<int, Afdeling>();
            //var afdelingen = await connection.QueryAsync<Afdeling, Medewerker, Afdeling>(
            //    AfdelingQuery(),
            //    (afdeling, medewerker) =>
            //    {
            //        if (!afdelingenDictionary.TryGetValue(afdeling.Sleutel, out var currentAfdeling))
            //        {
            //            currentAfdeling = afdeling;
            //            afdelingenDictionary.Add(currentAfdeling.Sleutel, currentAfdeling);
            //        }

            //        currentAfdeling.Medewerkers?.Add(medewerker);
            //        return currentAfdeling;
            //    },
            //    splitOn: "AfdelingId"
            //);

            //return afdelingen.Distinct().AsQueryable();

            connection.Open();
            var populatedModel = new Dictionary<int, Afdeling>();

            var query = await connection.QueryAsync<Afdeling, Medewerker, Bedrijf, Afdeling>(
                AfdelingQuery(),
                (afdeling, medewerker, bedrijf) =>
                {
                    if (!populatedModel.TryGetValue(afdeling.Sleutel, out var currentAfdeling))
                    {
                        currentAfdeling = afdeling;
                        populatedModel.Add(currentAfdeling.Sleutel, currentAfdeling);
                    }
                    //Medewerkers
                    currentAfdeling.Medewerkers?.Add(medewerker);
                    //Bedrijf
                    currentAfdeling.Bedrijf = bedrijf;
                    return currentAfdeling;
                },
                splitOn: "AfdelingId,BedrijfId"
            );

            return populatedModel.Values.AsQueryable();
            //return query.Distinct().AsQueryable();
        }
        catch
        {
            return default!;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();

        }
    }

    public static async Task<Afdeling> GetAfdelingFromDB(IDbConnection connection, int key)
    {
        try
        {
            connection.Open();
            var afdelingenDictionary = new Dictionary<int, Afdeling>();
            var afdelingen = await connection.QueryAsync<Afdeling, Medewerker, Afdeling>(
                AfdelingQuery(),
                (afdeling, medewerker) =>
                {
                    if (!afdelingenDictionary.TryGetValue(afdeling.Sleutel, out var currentAfdeling))
                    {
                        currentAfdeling = afdeling;
                        afdelingenDictionary.Add(currentAfdeling.Sleutel, currentAfdeling);
                    }

                    currentAfdeling.Medewerkers?.Add(medewerker);
                    return currentAfdeling;
                },
                new { key },
                splitOn: "AfdelingId"
            );

            return afdelingen.Distinct().FirstOrDefault() ?? default!;
        }
        catch
        {
            return default!;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }
    }

    public static async Task<IQueryable<Medewerker>> GetMedewerkersFromDB(IDbConnection connection, int key)
    {
        try
        {
            connection.Open();
            var medewerkers = await connection.QueryAsync<Medewerker>(MedewerkerQuery(), new { key });
            return medewerkers.AsQueryable();
        }
        catch
        {
            return default!;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }
    }

    public static async Task<Bedrijf> GetBedrijfFromDB(IDbConnection connection, int key)
    {
        try
        {
            connection.Open();
            var bedrijf = await connection.QueryFirstOrDefaultAsync<Bedrijf>(BedrijfQuery(), new { key });
            return bedrijf ?? default!;
        }
        catch
        {
            return default!;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }
    }

    #region Get Afdelingen
    //public class CustomRequest
    //{
    //    public string Test { get; set; }
    //}

    //[Endpoint<Afdeling>("afdelingenTest", "CustomRequest", EndpointMethod.Get)]
    //[CFW.ODataCore.Attributes.Class.EndpointAuthorize]
    //public class GenericHandler(IDbConnection connection) : CFW.ODataCore.Interfaces.Endpoints.IEndpointRequestHandler<CustomRequest>
    //{
    //    public async Task<MediatrResult<dynamic>> Handle(CustomRequest request, CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            //Haal mijn data op, op de manier zoals wij dat fijn vinden
    //            var data = await GetAfdelingenFromDB(connection);
    //            //Apply Odata AfdelingQuery Options to the data
    //            return new MediatrResult<dynamic>
    //            {
    //                IsSuccess = true,
    //                Data = data,
    //                HttpStatusCode = System.Net.HttpStatusCode.OK
    //            };
    //        }
    //        catch (Exception ex)
    //        {
    //            return new MediatrResult<dynamic>
    //            {
    //                IsSuccess = false,
    //                Exception = ex,
    //                Message = ex.Message,
    //                CustomResult = @"{ ""test"":""Deze functie is fout gegaan!""}",
    //                HttpStatusCode = System.Net.HttpStatusCode.NotAcceptable
    //            };
    //        }
    //    }
    //}

    [Endpoint<Afdeling>(EndpointMethod.Get)]
    public class GetHandler(IDbConnection connection) : IEndpointGetHandler<Afdeling>
    {
        public async Task<IMediatrResult<dynamic>> Handle(IODataQueryOptionsWithPageSize<Afdeling> options, CancellationToken cancellationToken)
        {

            //Haal mijn data op, op de manier zoals wij dat fijn vinden
            var data = await GetAfdelingenFromDB(connection);

            return options.ApplyODataOptions(data);
        }
    }
    #endregion

    #region Get Afdeling by Key
    [Endpoint<Afdeling, int>(EndpointMethod.Get)]
    public class GetByKeyHandler(IDbConnection connection) : IEndpointGetByKeyHandler<Afdeling, int>
    {
        public async Task<IMediatrResult<dynamic>> Handle(int key, IODataQueryOptionsWithPageSize<Afdeling> options, CancellationToken cancellationToken)
        {
            //Haal mijn data op, op de manier zoals wij dat fijn vinden
            var data = await GetAfdelingFromDB(connection, key);
            data.DatumOnly = new DateOnly(12, 12, 12);
            data.Time2 = new TimeOnly(1, 1, 1);
            data.TestFlag = FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E | FlagTest.F;
            data.Datum = DateTimeOffset.Now;
            data.Datum3 = new(DateTime.Now.Ticks, DateTimeKind.Local);
            data.Rechten = Rechten.Read | Rechten.Write;
            data.TestEnum = Test32.E;

            //Apply Odata AfdelingQuery Options to the data
            return options.ApplyODataOptions(data);
        }
    }
    #endregion

    #region Patch / Post / Put
    [Endpoint<Afdeling>(EndpointMethod.Post)]
    public class PostAfdelingHandler : IEndpointPostHandler<Afdeling>
    {
        async Task<IMediatrResult<dynamic>> IEndpointPostHandler<Afdeling>.Handle(Delta<Afdeling> domainObjectDelta, IODataQueryOptionsWithPageSize<Afdeling> options, CancellationToken cancellationToken)
        {
            return await MediatrResultFactory.CreateProblem(HttpStatusCode.BadRequest, "Not implemented yet");

            try
            {
                var s = domainObjectDelta.ValidateModel(ModelValidationMode.Post);

                domainObjectDelta.TryPost(out Afdeling afdeling);
                return options.ApplyODataOptions(afdeling);
            }
            catch (Exception ex)
            {
                return await MediatrResultFactory.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }

    [Endpoint<Afdeling, int>(EndpointMethod.Patch)]
    public class PatchAfdelingHandler(IDbConnection connection) : IEndpointPatchHandler<Afdeling, int>
    {
        async Task<IMediatrResult<dynamic>> IEndpointPatchHandler<Afdeling, int>.Handle(int key, Delta<Afdeling> domainObjectDelta, IODataQueryOptionsWithPageSize<Afdeling> options, CancellationToken cancellationToken)
        {
            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Patch);

                domainObjectDelta.Patch(origineel);
                return options.ApplyODataOptions(origineel);
            }
            catch (Exception ex)
            {
                return await MediatrResultFactory.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }

            //These are all properties that COULD be patched but still we need to omit the ones that contain the default value or are null
            //var properties = GetProperties<Afdeling>(OperationType.Patch, entity);

            //Haal origineel op
            //Nu nog de Delta Bepalen en de juiste properties eruit halen
            //en dan het update statement dynamisch creeereren
            //en uitvoeren.

            //Stukje met etag lezen en toevoegen
            //var etag = Request.Headers["If-Match"].FirstOrDefault();



            //var delta = CompareObjects<Afdeling>(HTTPOperation.Patch, origineel, entity);

            // var y = delta.GetChangedPropertyNames();
        }
    }

    [Endpoint<Afdeling, int>(EndpointMethod.Put)]
    public class PutAfdelingHandler(IDbConnection connection) : IEndpointPutHandler<Afdeling, int>
    {
        public async Task<IMediatrResult<dynamic>> Handle(int key, Delta<Afdeling> domainObjectDelta, IODataQueryOptionsWithPageSize<Afdeling> options, CancellationToken cancellationToken)
        {
            MediatrResult<dynamic> result = new();

            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Put);

                domainObjectDelta.Put(origineel);
                return options.ApplyODataOptions(origineel);
            }
            catch (Exception ex)
            {
                return await MediatrResultFactory.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }

            //These are all properties that COULD be patched but still we need to omit the ones that contain the default value or are null
            //var properties = GetProperties<Afdeling>(OperationType.Patch, entity);

            //Haal origineel op
            //Nu nog de Delta Bepalen en de juiste properties eruit halen
            //en dan het update statement dynamisch creeereren
            //en uitvoeren.

            //Stukje met etag lezen en toevoegen
            //var etag = Request.Headers["If-Match"].FirstOrDefault();



            //var delta = CompareObjects<Afdeling>(HTTPOperation.Patch, origineel, entity);

            // var y = delta.GetChangedPropertyNames();
        }
    }
    #endregion

    #region Delete Afdeling
    [Endpoint<Afdeling, int>(EndpointMethod.Delete)]
    public class DeleteAfdelingHandler : IEndpointDeleteHandler<Afdeling, int>
    {
        public async Task<IMediatrResult<dynamic>> Handle(int key, CancellationToken cancellationToken)
        {
            try
            {
                return await MediatrResultFactory.CreateSuccess(default!, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await MediatrResultFactory.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }
    #endregion

    #region Navigation Objects
    [Endpoint<Afdeling, int, Medewerker>(EndpointMethod.Get, "medewerkers")]
    public class GetMedewerkersHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Medewerker>
    {
        public async Task<IMediatrResult<dynamic>> Handle(int key, Type TDomainObject, IODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
        {
            var data = await GetMedewerkersFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }

    [Endpoint<Afdeling, int, Bedrijf>(EndpointMethod.Get, "bedrijf")]
    public class GetBedrijfHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Bedrijf>
    {
        public async Task<IMediatrResult<dynamic>> Handle(int key, Type TDomainObject, IODataQueryOptionsWithPageSize<Bedrijf> options, CancellationToken cancellationToken)
        {
            var data = await GetBedrijfFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }
    #endregion
}