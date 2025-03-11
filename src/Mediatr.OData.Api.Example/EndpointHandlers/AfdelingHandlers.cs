using Dapper;
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Example.DomainObjects;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
using System.Data;

namespace Mediatr.OData.Api.Example.EndpointHandlers;

public class AfdelingHandlers
{
    public static string Query(bool withKey = false)
    {
        string sql = @"
            SELECT
                a.Sleutel,
                a.Name,
                a.Description,
                a.Sleutel AS AfdelingId,
                m.AfdelingId,                    
                m.Id,
                m.Name,
                m.Description
            FROM Medewerkers m
            LEFT JOIN Afdeling a
            ON m.AfdelingId = a.Sleutel
            ";
        if (withKey)
        {
            sql += "WHERE a.Sleutel = @key";
        }

        return sql;
    }

    public static async Task<IQueryable<Afdeling>> GetAfdelingenFromDB(IDbConnection connection)
    {
        try
        {
            connection.Open();
            var afdelingenDictionary = new Dictionary<int, Afdeling>();
            var afdelingen = await connection.QueryAsync<Afdeling, Medewerker, Afdeling>(
                Query(),
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
                splitOn: "AfdelingId"
            );

            return afdelingen.Distinct().AsQueryable();
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
                Query(),
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

    #region Get Afdelingen
    //public class CustomRequest
    //{
    //    public string Test { get; set; }
    //}

    //[Endpoint<Afdeling>("afdelingenTest", "CustomRequest", EndpointMethod.Get)]
    //[CFW.ODataCore.Attributes.Class.EndpointAuthorize]
    //public class GenericHandler(IDbConnection connection) : CFW.ODataCore.Interfaces.Endpoints.IEndpointRequestHandler<CustomRequest>
    //{
    //    public async Task<Result<dynamic>> Handle(CustomRequest request, CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            //Haal mijn data op, op de manier zoals wij dat fijn vinden
    //            var data = await GetAfdelingenFromDB(connection);
    //            //Apply Odata Query Options to the data
    //            return new Result<dynamic>
    //            {
    //                IsSuccess = true,
    //                Data = data,
    //                HttpStatusCode = System.Net.HttpStatusCode.OK
    //            };
    //        }
    //        catch (Exception ex)
    //        {
    //            return new Result<dynamic>
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

    [Endpoint<Afdeling>("afdelingen", EndpointMethod.Get)]
    public class GetHandler(IDbConnection connection) : IEndpointGetHandler<Afdeling>
    {
        public async Task<Result<dynamic>> Handle(ODataQueryOptionsWithPageSize<Afdeling> options, CancellationToken cancellationToken)
        {
            try
            {
                //Haal mijn data op, op de manier zoals wij dat fijn vinden
                var data = await GetAfdelingenFromDB(connection);

                //Apply Odata Query Options to the data
                return options.ApplyODataOptions(data);
            }
            catch (Exception ex)
            {
                return new Result<dynamic>
                {
                    IsSuccess = false,
                    Exception = ex,
                    Message = ex.Message,
                    CustomResult = @"{ ""test"":""Deze functie is fout gegaan!""}",
                    HttpStatusCode = System.Net.HttpStatusCode.NotAcceptable
                };

            }
        }
    }
    #endregion

    #region Get Afdeling by Key
    [Endpoint<Afdeling, int>("afdelingen", EndpointMethod.Get)]
    public class GetByKeyHandler(IDbConnection connection) : IEndpointGetByKeyHandler<Afdeling, int>
    {
        public async Task<Result<dynamic>> Handle(int key, ODataQueryOptionsWithPageSize<Afdeling> options, CancellationToken cancellationToken)
        {
            try
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

                //Apply Odata Query Options to the data
                return options.ApplyODataOptions(data);
            }
            catch (Exception ex)
            {
                return new Result<dynamic>
                {
                    IsSuccess = false,
                    Exception = ex,
                    Message = ex.Message,
                    CustomResult = @"{ ""test"":""Deze functie is fout gegaan!""}",
                    HttpStatusCode = System.Net.HttpStatusCode.NotAcceptable
                };
            }
        }
    }
    #endregion

    #region Patch / Post / Put
    [Endpoint<Afdeling>("afdelingen", EndpointMethod.Post)]
    public class PostAfdelingHandler : IEndpointPostHandler<Afdeling>
    {
        async Task<Result<dynamic>> IEndpointPostHandler<Afdeling>.Handle(Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            Result<dynamic> x = new();
            await Task.CompletedTask;
            try
            {
                var s = domainObjectDelta.ValidateModel(ModelValidationMode.Post);

                Afdeling afdeling = domainObjectDelta.Post();
                domainObjectDelta.TryPost(out afdeling);
                //
                //Do something with the entity
                //x.Data = entity;
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
            return x;
        }
    }

    [Endpoint<Afdeling, int>("afdelingen", EndpointMethod.Patch)]
    public class PatchAfdelingHandler(IDbConnection connection) : IEndpointPatchHandler<Afdeling, int>
    {
        async Task<Result<dynamic>> IEndpointPatchHandler<Afdeling, int>.Handle(int key, Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            Result<dynamic> result = new();

            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Patch);

                //Verwijder de key van de Delta Properties
                //var keyPropertyName = KeyProperty<Afdeling>().Name;
                //delta.TrySetPropertyValue(keyPropertyName, null);
                domainObjectDelta.Patch(origineel);

                result.Data = origineel;
                result.IsSuccess = true;
                result.HttpStatusCode = System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                result = new()
                {
                    IsSuccess = false,
                    Exception = ex,
                    Message = ex.Message,
                    CustomResult = @"{ ""test"":""Deze functie is fout gegaan!""}",
                    HttpStatusCode = System.Net.HttpStatusCode.NotAcceptable
                };
            }
            return result;
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

    [Endpoint<Afdeling, int>("afdelingen", EndpointMethod.Put)]
    public class PutAfdelingHandler(IDbConnection connection) : IEndpointPutHandler<Afdeling, int>
    {
        public async Task<Result<dynamic>> Handle(int key, Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            Result<dynamic> result = new();

            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Put);

                //Verwijder de key van de Delta Properties
                //var keyPropertyName = KeyProperty<Afdeling>().Name;
                //delta.TrySetPropertyValue(keyPropertyName, null);

                result.IsSuccess = true;
                result.HttpStatusCode = System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                result = new()
                {
                    IsSuccess = false,
                    Exception = ex,
                    Message = ex.Message,
                    CustomResult = @"{ ""test"":""Deze functie is fout gegaan!""}",
                    HttpStatusCode = System.Net.HttpStatusCode.NotAcceptable
                };
            }
            return result;
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
    [Endpoint<Afdeling, int>("afdelingen", EndpointMethod.Delete)]
    public class DeleteAfdelingHandler : IEndpointDeleteHandler<Afdeling, int>
    {
        public async Task<Result<dynamic>> Handle(int key, CancellationToken cancellationToken)
        {
            Result<dynamic> x = new();
            await Task.CompletedTask;
            try
            {
                //Do something with the entity
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
            return x;
        }
    }
    #endregion

    #region Navigation Objects
    [Endpoint<Afdeling, int, Medewerker>("afdelingen", "medewerkers/", EndpointMethod.Get)]
    public class GetMedewerkersHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Medewerker>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
        {
            //We can use the TDomainObjectType to find the name of the key and then relate it to the TNavigationObjectForeignKey ;)

            Result<dynamic> x = new();
            await Task.CompletedTask;
            try
            {
                connection.Open();
                //Due to the Left join we need to Distinct the primary object
                var data = await connection.QueryAsync<Medewerker>("SELECT * FROM dbo.Medewerkers WHERE AfdelingId = @AfdelingId", new { AfdelingId = key });
                //Haal mijn data op, op de manier zoals wij dat fijn vinden
                ODataQuerySettings settings = new()
                {
                    PageSize = options.PageSize
                };
                var result = options.ApplyTo(data.AsQueryable(), settings);
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

    [Endpoint<Afdeling, int, Bedrijf>("afdelingen", "bedrijf", EndpointMethod.Get)]
    public class GetBedrijfHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Bedrijf>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Bedrijf> options, CancellationToken cancellationToken)
        {
            Result<dynamic> x = new();
            await Task.CompletedTask;
            try
            {
                connection.Open();
                //Due to the Left join we need to Distinct the primary object
                var data = await connection.QueryAsync<Bedrijf>("SELECT * FROM dbo.Bedrijf WHERE Id = (SELECT TOP 1 BedrijfId FROM dbo.Afdeling WHERE Id = @AfdelingId)", new { AfdelingId = key });
                //Haal mijn data op, op de manier zoals wij dat fijn vinden
                ODataQuerySettings settings = new()
                {
                    PageSize = options.PageSize
                };
                var result = options.ApplyTo(data.AsQueryable(), settings);
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
    #endregion 
}
