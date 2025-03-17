#OData Minimal API

**OData Minimal API** is a lightweight framework build on **ASP.NET Core OData**, fully compatible with **.NET Core 8**. 

It is designed to simplify the development of API's by focusing on clean extensible handlers instead of manually managing HTTP status code and request-response models. It leverages a **Result Pattern** to streamline operation outcomes and take advantage of the latest improvements in **.NET Core 8**.

https://devblogs.microsoft.com/odata/build-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice/
---
This package supports the full OData syntax and is extended to also provide data types and ETag attributes.

On top of the OData support, out of the box a documentation and test UI is added out of the box. It looks a lot like the UI for the Microsoft Graph API explorer and even shows implementation examples in the coding language of your choice.

Finally we have included the optional authentication and authorization features for Microsoft Entra for both the usage of the UI and the actual Api's themselves. Which can be configured independent from each other.

### Step 1 Installation
Using .NET CLI:
```bash
dotnet add package Mediatr.OData.Api
```

Using Package Manager:
```bash
Install-Package Mediatr.OData.Api
```

### Step 2 Add the necessary command to activate the package
In the ASP.NET Core Mimimal API project, update your `Program.cs` as below:

```C#
using Mediatr.OData.Api.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

//Add any database or data provider you need to fetch, update, 
//create or delete data, to be used by dependency injection to suit your needs.
//But is certainly not required.
builder.Services.AddTransient<IDbConnection>(x => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

//This is the line of code where the actual magic happens.
builder.CreateAndRegisterODataRoutes();

var app = builder.Build();

//This is the line of code where the runtime generated endpoint services are mapped to the API routes.
app.AttachODataRoutes();

app.Run();
```
So you actually only need 2 lines of code to make this work.

###Step 3 Add the configuration for the OData API configuration to the appsettings.json
Add the following section to your appsettings.json to configure the OData Api module

```Json
  "OData": {
    "Title": "OData API v1.0.2",    //Use any title you like to display in the browsers titlebar
    "SecureAPI": false,             //If you want to secure the API usage with Entra set this flag to true (and include step 4)
    "SecureWebInterface": false,    //If you want to secure the UI usage with Entra set this flag to true (and include step 4)
    "RoutePrefix": "odata",         //This is the prefix from the base url (http://somedomain/<prefix> that will be used 
    "PageSize": 100,                //This is the maximum number of dataobjects to fetch per call. 
    "UseHttpsRedirection": true     //If you want any HTTP request automatic to be forwarded to HTTPS set this flag to true
  },
```
For more detailed information on the appsettings please take a look at <reference to Appsettings.json>

###Step 4 (optional) Add the configuration for the Entra authentication and authorization to your appsettings.json
```Json
  "Entra": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<Tenant Domain (entra)>",                 //Your offical Entra domain <somedomain.onmicrosoft.com>
    "TenantId": "<TenantId>",                            //Your tenantId
    "ClientId": "<ClientId>",                            //The clientId of the App registration
    "ClientSecret": "<ClientSecret>",                    //The clientSecret of the App registration
    "CallbackPath": "/signin-oidc"                       //This callback path is mandantory when you want to secure the UI
  }
```
For more detailed information on using Entra for Authentication and Authorization please look at  <reference to Securing things with Entra>

###Step 5 Create a DomainModel
Make sure your classes implement the interface **IDomainObject** or **IDomainObject<Key>**. The framework checks if an object has one of these interfaces implemented. Objects without these interfaces implemented cannot be handled by the **OData Api framework**.

As an example:

file : Example\DomainModel\Departments.cs
```C#
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Mediatr.OData.Api.Example.DomainModel;

public class Department: IDomainObject<int>
{
    public int Id{ get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public Company? Company { get; set; } = default!;
    public ICollection<Employee>? Employees { get; set; } = [];
}
```
file : Example\DomainModel\Employee.cs
```C#
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Mediatr.OData.Api.Example.DomainModel;

public class Employee: IDomainObject<int>
{
    public int Id{ get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateOnly BirthDay {get; set; }
    public Department? Department {get; set; }
}
```
file : Example\DomainModel\Company.cs
```C#
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Mediatr.OData.Api.Example.DomainModel;

public class Company: 
{
    public int Id{ get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Department>? Departments {get; set;}
}
```
Please take a look at DomainObjects <reference to DomainObjects> for the usage of all attribute options and the default determinations.

In the above example the DomainObject Company does **not** have the **interface IDomainObject<Key>** implemented, this means it is still part of the domain model but you **can not** create an API endpoint on Company. But you can attach it as an $Expand to any OData query and result to the Endpoints for Department and Employee. 

###Step 6 Create a GET handler implementation to actually have 2 endpoints
Now we work our magic to create and endpoint handler that actually processes a request and returns data with full OData support without you having to code some OData implementation or any HTTP integration.
All you need to focus on is fetching your data. I have chosen for a dapper implementation in my example but you can use any provider you like as long as the data is transformed into the domainmodel and the result is IQueryable (if you have multiple records).

file : Example\EndpointHandlers\DepartmentHandlers.cs
```C#
using Dapper;
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Example.DomainObjects;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Deltas;
using System.Data;
using System.Net;

namespace Mediatr.OData.Api.Example.EndpointHandlers;

//This is the container class that holds (in this case) all endpoints for the domainobject 'Department'

//For easy of use you can add this attribute all endpoints inside this container class
//So the endpoints will be http(s)://somedomain/odata/departments 
//This way you don't have to specify the route on each endpoint (saves a parameter)
[EndpointGroup("departments")] 
public class DepartmentHandlers
{
    //Some helper function to get the query for the data
    public static string DepartmentQuery(bool withKey = false)
    { ...}

    //Some helper function to get the query for the data
    public static string EmployeeQuery()
    { .. }

    //Some helper function to get the query for the data   
    public static string CompanyQuery()
    { ... }

    //Some helper function to get the query for the data
    public static async Task<IQueryable<Department>> GetDepartmentsFromDB(IDbConnection connection)
    { ... }

    //Some helper function to get the query for the data
    public static async Task<Department> GetDepartmentFromDB(IDbConnection connection, int key)
    { ... }
    
    //Some helper function to get the query for the data
    public static async Task<IQueryable<Employee>> GetEmployeesFromDB(IDbConnection connection, int key)
    { ... }

    //Some helper function to get the query for the data
    public static async Task<Company> GetCompaniesFromDB(IDbConnection connection, int key)
    { ... }

    //Use the attribute Endpoint<DomainObject> with the Method.Get to designate that it will 
    //fetch 1 or more departments.
    //This results in the endpoint:
    //GET http(s):/somedomain/odata/departments 
    [Endpoint<Department>(EndpointMethod.Get)]
    //Implement the identical interface for this endpoint : IEndpointGetHandler<DomainObject>
    //To bind things together
    public class GetHandler(IDbConnection connection) : IEndpointGetHandler<Department>
    {
        public async Task<Result<dynamic>> Handle(ODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
        {

            //Get the data as IQuerable<Department>
            var data = await GetDepartmentsFromDB(connection);
     
            //Apply OData syntax and return the result
            return options.ApplyODataOptions(data);
        }
    }

    //Use the attribute Endpoint<DomainObject,keytype> with the Method.Get to designate
    //that it will fetch 1 specific Department where it's Id = {key}
    //This results in the endpoint: 
    //GET http(s)://somedomain/odata/departments/{key}   
    [Endpoint<Department, int>(EndpointMethod.Get)]
    //Implement the identical interface for this endpoint : IEndpointGetHandler<DomainObject,key>
    //To bind things together
    public class GetByKeyHandler(IDbConnection connection) : IEndpointGetByKeyHandler<Department, int>
    {
        public async Task<Result<dynamic>> Handle(int key, ODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
        {
            //Get the data as Department
            var data = await GetDepartmentFromDB(connection, key);
            //Apply Odata syntax and return the result
            return options.ApplyODataOptions(data);
        }
    }
}
```
If you run your project now you will have successful integrated the 2 GET endpoints for the Department domainObject including full OData support.

###Step 6 Create a POST handler implementation
If you want to add the creation of a Department (Post) you can add the following snippet to the container class:
```C#
    [Endpoint<Department>(EndpointMethod.Post)]
    public class PostDepartmentHandler : IEndpointPostHandler<Department>
    {
        async Task<Result<dynamic>> IEndpointPostHandler<Department>.Handle(Delta<Department> domainObjectDelta, CancellationToken cancellationToken)
        {
            try
            {

                domainObjectDelta.TryPost(out Department department);
                
                //Here you use your own code to actually save the object to your datastorage
                
                //And here you return the newly created domainObject
                return await Result.CreateSuccess(department, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }
```
We actually supply the Post method with a Delta<T> object, this gives the TryPost method the ability
to validatte if all required fields (according to the domain model) are actually supplied.
And it operates under the assumption that the generation of a new Id/Key is done by the datastorage or your own code. 

So it is expected that the actual Id/Key of the department object is returned from your own code before we return the created object.

###Step 7 Create a PATCH or PUT handler implementation
Both the PATCH and the PUT handler also work with a Delta<T> object with the same assumptions as with Post handler. The actual difference is that it works with the Key definition (like with the GetByKey handler) and any Id/Key that is supplied in the body is ignored automatic since the actual id/key is supplied in the route. And we use TryPatch and TryPut instead of TryPost. 
Furthermore if we follow strict REST guidelines Patch would be used for partial updates and put for complete domainobjects. 

In this implementation the choice was explicitly made to always supply both Full as Partial updates. Even though this formally breaks the REST guidelines it didn't seem logic to force a full object update 
since we have full support that optional fields are ignored both when fetching data and when posting/putting data to keep the model data always clean and not clutter things with NULL values.

And example of the implementations of PUT and PATCH are below: 
```C#
    [Endpoint<Department, int>(EndpointMethod.Patch)]
    public class PatchDepartmentHandler(IDbConnection connection) : IEndpointPatchHandler<Department, int>
    {
        async Task<Result<dynamic>> IEndpointPatchHandler<Department, int>.Handle(int key, Delta<Department> domainObjectDelta, CancellationToken cancellationToken)
        {
            try
            {
                //Fetch the original object
                var original= await GetDepartmentFromDB(connection, key);

                //Patch the original object with the changed values
                domainObjectDelta.TryPatch(original);

                //Return the patched result domain object
                return await Result.CreateSuccess(original, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }

    [Endpoint<Department, int>(EndpointMethod.Put)]
    public class PutDepartmentHandler(IDbConnection connection) : IEndpointPutHandler<Department, int>
    {
        public async Task<Result<dynamic>> Handle(int key, Delta<Department> domainObjectDelta, CancellationToken cancellationToken)
        {
            Result<dynamic> result = new();

            try
            {
                //Fetch the original object
                var original= await GetDepartmentFromDB(connection, key);

                //Patch the original object
                domainObjectDelta.TryPut(original);

                //Return the patched object
                return await Result.CreateSuccess(original, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }
```
###Step 8 Create a Delete handler implementation
When you want to implement the delete action on the Api you need to implement the Delete handler as shown below:

```C#
    [Endpoint<Department, int>(EndpointMethod.Delete)]
    public class DeleteDepartmentHandler : IEndpointDeleteHandler<Department, int>
    {
        public async Task<Result<dynamic>> Handle(int key, CancellationToken cancellationToken)
        {
            try
            {
                //Here you implement your own code to remove the specific Department with id/key from your data storage.

                //Return the success message that the removal was successful 
                return await Result.CreateSuccess(default!, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }
```

```C#
using Dapper;
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Example.DomainObjects;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Deltas;
using System.Data;
using System.Net;

namespace Mediatr.OData.Api.Example.EndpointHandlers;

//This is the container class that holds (in this case) all endpoints for the domainobject 'Department'

//For easy of use you can add this attribute all endpoints inside this container class
//So the endpoints will be http(s)://somedomain/odata/departments 
//This way you don't have to specify the route on each endpoint (saves a parameter)
[EndpointGroup("departments")] 
public class DepartmentHandlers
{
    //Some helper function to get the query for the data
    public static string DepartmentQuery(bool withKey = false)
    { ...}

    //Some helper function to get the query for the data
    public static string EmployeeQuery()
    { .. }

    //Some helper function to get the query for the data   
    public static string CompanyQuery()
    { ... }

    //Some helper function to get the query for the data
    public static async Task<IQueryable<Department>> GetDepartmentsFromDB(IDbConnection connection)
    { ... }

    //Some helper function to get the query for the data
    public static async Task<Department> GetDepartmentFromDB(IDbConnection connection, int key)
    { ... }
    
    //Some helper function to get the query for the data
    public static async Task<IQueryable<Employee>> GetEmployeesFromDB(IDbConnection connection, int key)
    { ... }

    //Some helper function to get the query for the data
    public static async Task<Company> GetCompaniesFromDB(IDbConnection connection, int key)
    { ... }

    [Endpoint<Department>(EndpointMethod.Get)]
    public class GetHandler(IDbConnection connection) : IEndpointGetHandler<Department>
    {
        public async Task<Result<dynamic>> Handle(ODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
        {

            //Get the data as IQuerable<Department>
            var data = await GetDepartmentsFromDB(connection);
     
            //Apply OData syntax and return the result
            return options.ApplyODataOptions(data);
        }
    }

    [Endpoint<Department, int>(EndpointMethod.Get)]
    public class GetByKeyHandler(IDbConnection connection) : IEndpointGetByKeyHandler<Department, int>
    {
        public async Task<Result<dynamic>> Handle(int key, ODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
        {
            //Get the data as Department
            var data = await GetDepartmentFromDB(connection, key);
            //Apply Odata syntax and return the result
            return options.ApplyODataOptions(data);
        }
    }

    #region Patch / Post / Put
    [Endpoint<Afdeling>(EndpointMethod.Post)]
    public class PostAfdelingHandler : IEndpointPostHandler<Afdeling>
    {
        async Task<Result<dynamic>> IEndpointPostHandler<Afdeling>.Handle(Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            return await Result.CreateProblem(HttpStatusCode.BadRequest, "Not implemented yet");

            try
            {
                var s = domainObjectDelta.ValidateModel(ModelValidationMode.Post);

                domainObjectDelta.TryPost(out Afdeling afdeling);

                return await Result.CreateSuccess(afdeling, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }

    [Endpoint<Afdeling, int>(EndpointMethod.Patch)]
    public class PatchAfdelingHandler(IDbConnection connection) : IEndpointPatchHandler<Afdeling, int>
    {
        async Task<Result<dynamic>> IEndpointPatchHandler<Afdeling, int>.Handle(int key, Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Patch);

                domainObjectDelta.Patch(origineel);
                return await Result.CreateSuccess(origineel, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
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
        public async Task<Result<dynamic>> Handle(int key, Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            Result<dynamic> result = new();

            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Put);

                domainObjectDelta.Put(origineel);
                return await Result.CreateSuccess(origineel, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
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
        public async Task<Result<dynamic>> Handle(int key, CancellationToken cancellationToken)
        {
            try
            {
                return await Result.CreateSuccess(default!, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }
    #endregion

    #region Navigation Objects
    [Endpoint<Afdeling, int, Medewerker>(EndpointMethod.Get, "medewerkers")]
    public class GetMedewerkersHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Medewerker>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
        {
            var data = await GetMedewerkersFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }

    [Endpoint<Afdeling, int, Bedrijf>(EndpointMethod.Get, "bedrijf")]
    public class GetBedrijfHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Bedrijf>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Bedrijf> options, CancellationToken cancellationToken)
        {
            var data = await GetBedrijfFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }
    #endregion
}
```

```C#
using Dapper;
using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Example.DomainObjects;
using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Deltas;
using System.Data;
using System.Net;

namespace Mediatr.OData.Api.Example.EndpointHandlers;

//This is the container class that holds (in this case) all endpoints for the domainobject 'Department'

//For easy of use you can add this attribute all endpoints inside this container class
//So the endpoints will be http(s)://somedomain/odata/departments 
//This way you don't have to specify the route on each endpoint (saves a parameter)
[EndpointGroup("departments")] 
public class DepartmentHandlers
{
    //Some helper function to get the query for the data
    public static string DepartmentQuery(bool withKey = false)
    { ...}

    //Some helper function to get the query for the data
    public static string EmployeeQuery()
    { .. }

    //Some helper function to get the query for the data   
    public static string CompanyQuery()
    { ... }

    //Some helper function to get the query for the data
    public static async Task<IQueryable<Department>> GetDepartmentsFromDB(IDbConnection connection)
    { ... }

    //Some helper function to get the query for the data
    public static async Task<Department> GetDepartmentFromDB(IDbConnection connection, int key)
    { ... }
    
    //Some helper function to get the query for the data
    public static async Task<IQueryable<Employee>> GetEmployeesFromDB(IDbConnection connection, int key)
    { ... }

    //Some helper function to get the query for the data
    public static async Task<Company> GetCompaniesFromDB(IDbConnection connection, int key)
    { ... }

    [Endpoint<Department>(EndpointMethod.Get)]
    public class GetHandler(IDbConnection connection) : IEndpointGetHandler<Department>
    {
        public async Task<Result<dynamic>> Handle(ODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
        {

            //Get the data as IQuerable<Department>
            var data = await GetDepartmentsFromDB(connection);
     
            //Apply OData syntax and return the result
            return options.ApplyODataOptions(data);
        }
    }

    [Endpoint<Department, int>(EndpointMethod.Get)]
    public class GetByKeyHandler(IDbConnection connection) : IEndpointGetByKeyHandler<Department, int>
    {
        public async Task<Result<dynamic>> Handle(int key, ODataQueryOptionsWithPageSize<Department> options, CancellationToken cancellationToken)
        {
            //Get the data as Department
            var data = await GetDepartmentFromDB(connection, key);
            //Apply Odata syntax and return the result
            return options.ApplyODataOptions(data);
        }
    }

    #region Patch / Post / Put
    [Endpoint<Afdeling>(EndpointMethod.Post)]
    public class PostAfdelingHandler : IEndpointPostHandler<Afdeling>
    {
        async Task<Result<dynamic>> IEndpointPostHandler<Afdeling>.Handle(Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            return await Result.CreateProblem(HttpStatusCode.BadRequest, "Not implemented yet");

            try
            {
                var s = domainObjectDelta.ValidateModel(ModelValidationMode.Post);

                domainObjectDelta.TryPost(out Afdeling afdeling);

                return await Result.CreateSuccess(afdeling, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }

    [Endpoint<Afdeling, int>(EndpointMethod.Patch)]
    public class PatchAfdelingHandler(IDbConnection connection) : IEndpointPatchHandler<Afdeling, int>
    {
        async Task<Result<dynamic>> IEndpointPatchHandler<Afdeling, int>.Handle(int key, Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Patch);

                domainObjectDelta.Patch(origineel);
                return await Result.CreateSuccess(origineel, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
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
        public async Task<Result<dynamic>> Handle(int key, Delta<Afdeling> domainObjectDelta, CancellationToken cancellationToken)
        {
            Result<dynamic> result = new();

            try
            {
                //Originele record
                var origineel = await GetAfdelingFromDB(connection, key);

                domainObjectDelta.ValidateModel(ModelValidationMode.Put);

                domainObjectDelta.Put(origineel);
                return await Result.CreateSuccess(origineel, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
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
        public async Task<Result<dynamic>> Handle(int key, CancellationToken cancellationToken)
        {
            try
            {
                return await Result.CreateSuccess(default!, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return await Result.CreateProblem(HttpStatusCode.BadRequest, ex.Message, ex);
            }
        }
    }
    #endregion

    #region Navigation Objects
    [Endpoint<Afdeling, int, Medewerker>(EndpointMethod.Get, "medewerkers")]
    public class GetMedewerkersHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Medewerker>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Medewerker> options, CancellationToken cancellationToken)
        {
            var data = await GetMedewerkersFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }

    [Endpoint<Afdeling, int, Bedrijf>(EndpointMethod.Get, "bedrijf")]
    public class GetBedrijfHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Afdeling, int, Bedrijf>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Bedrijf> options, CancellationToken cancellationToken)
        {
            var data = await GetBedrijfFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }
    #endregion
}
```
```C#
TODO
```



---

## Key Features
1. [Handler support  API](#1-common-crud-entity-api)
   - [GET `/products` or `/products/{key}` with OData Query Options](#get-products-or-productskey-with-odata-query-options)
   - [POST `/products` with 1-Level Nested Relationships](#post-products-with-1-level-nested-relationships)
   - [PATCH `/products/{key}` with Partial Updates](#patch-productskey-with-partial-updates)
   - [DELETE `/products/{key}`](#delete-productskey)
2. [Authentication and Authorization](#4-authentication-and-authorization)


#### GET: Query and get by key with OData query options:

The following OData query options are supported for both:
- **GET `/products`**: Retrieve a list of all products with query capabilities.
- **GET `/products/{id}`**: Retrieve a specific product by ID with additional query capabilities.

| Query Option   | Example                               | Description                                   | SQL Output Example                          |
|----------------|---------------------------------------|-----------------------------------------------|---------------------------------------------|
| **$filter**    | `/products?$filter=price gt 50`       | Filter products with a price greater than $50. | `SELECT Id, Name, Price, CategoryId FROM Products WHERE Price > 50;` |
| **$select**    | `/products?$select=name,price`        | Select only the `name` and `price` fields.     | `SELECT Name, Price FROM Products;`         |
| **$orderby**   | `/products?$orderby=price desc`       | Order products by price in descending order.   | `SELECT Id, Name, Price, CategoryId FROM Products ORDER BY Price DESC;` |
| **$top**       | `/products?$top=5`                   | Retrieve the top 5 products.                  | `SELECT TOP 5 Id, Name, Price, CategoryId FROM Products;`             |
| **$skip**      | `/products?$skip=10`                 | Skip the first 10 products.                   | `SELECT Id, Name, Price, CategoryId FROM Products OFFSET 10 ROWS;`    |
| **$expand**    | `/products?$expand=category`         | Expand related `category` data for each product. | `SELECT p.Id, p.Name, p.Price, c.Id AS CategoryId, c.Name AS CategoryName FROM Products p JOIN Categories c ON p.CategoryId = c.Id;` |

---
#### POST `/products` with 1-Level nested relationships:

The framework supports creating:
1. **Main Entity**: Insert a single entity into the database.
2. **Child Entities (1 Level Nested)**: Handle child entities (either a single complex entity or a collection of entities) nested directly under the main entity.  
   - **Existing Child Entities**: If the child item's ID exists in the database, it is ignored.
   - **New Child Entities**: If the child item's ID is null or not in the database, it is inserted.

| Scenario                          | Request Body                                                                                                                                         | Explanation                                                                                                                                                          |
|-----------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Main Entity Only**              | ```{ "name": "Product A", "price": 150.0 } ```                                                                                                  | Inserts the `Product` entity without any nested relationships.                                                                                                       |
| **Main Entity with Single Child** | ```{ "name": "Product B", "price": 120.0, "category": { "name": "Electronics" } } ```                                                            | Inserts the `Product` entity and the `Category` entity if it doesn't exist. If the `Category` exists, it is associated with the `Product`.                          |
| **Main Entity with Child List**   | ```{ "name": "Product C", "price": 200.0, "orders": [ { "id": 1, "quantity": 2 }, { "quantity": 3 } ] } ```                                      | Inserts the `Product` entity. Existing `Orders` (with `id`) are associated, and new `Orders` (without `id`) are inserted and associated.                            |

---

#### PATCH `/products/{id}` with partial updates:

The PATCH method supports updating specific fields of an existing <strong>ROOT</strong> entity. The request body should contain only the fields that need to be updated.

| Scenario              | Request Body                                                                                         | SQL Generated                                                                                   | Explanation                                                                                  |
|-----------------------|-----------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| **Update Name Only**  | ```json { "name": "Updated Product A" } ```                                                         | ```sql UPDATE Products SET Name = 'Updated Product A' WHERE Id = 1; ```                        | Updates the `name` of the `Product` entity while leaving other fields unchanged.            |
| **Update Price Only** | ```json { "price": 175.0 } ```                                                                      | ```sql UPDATE Products SET Price = 175.0 WHERE Id = 2; ```                                     | Updates the `price` of the `Product` entity while leaving other fields unchanged.           |
| **Update Multiple**   | ```json { "name": "Updated Product B", "price": 200.0 } ```                                         | ```sql UPDATE Products SET Name = 'Updated Product B', Price = 200.0 WHERE Id = 3; ```        | Updates both `name` and `price` fields of the `Product` entity while leaving others intact. |

---
#### DELETE `/products/{id}`:

| Scenario              | SQL Generated                                                   | Explanation                                                                                  |
|-----------------------|-----------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| **Delete by ID**      | ```sql DELETE FROM Products WHERE Id = 1; ```                   | Deletes the `Product` entity with ID `1`.                                                   |

---

---
### **4. Authentication and Authorization**

The framework integrates seamlessly with ASP.NET Core's **authentication** and **authorization** mechanisms to provide secure access to entities and operations. This includes:

1. **Authentication**: Ensures that only authenticated users can access the application.
2. **Authorization**: Enforces role-based and method-specific access control, leveraging:
   - **`EntityAuthorize`**: Custom attribute for CRUD operations, inheriting from ASP.NET Core's `AuthorizeAttribute`.
   - **ASP.NET Core's `AuthorizeAttribute`**: Used for custom entity operations.

By following the ASP.NET Core model, the framework ensures compatibility with existing security configurations, such as `JwtBearer`, `CookieAuthentication`, or external providers (e.g., OAuth, OpenID Connect).

### Example: Configuring Authorization for an Entity
```csharp
[Entity("multi-authorizations")]
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.Query })] // Only authorized users can query the entity.
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.GetByKey }, Roles = TestUtils.AdminRole)] // Admin-only access for GetByKey.
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.Post }, Roles = $"{TestUtils.AdminRole},{TestUtils.SupperAdminRole}")] // Admin and SuperAdmin access for Post.
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.Delete }, Roles = TestUtils.SupperAdminRole)] // SuperAdmin-only access for Delete.
public class MultiAuthorization : IEntity<Guid>
{
    public Guid Id { get; set; }
}
```
---

