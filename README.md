# OData Minimal API

**OData Minimal API** is a lightweight framework build on **ASP.NET Core OData**, fully compatible with **.NET Core 8**. 

It is designed to simplify the development of API's by focusing on clean extensible handlers instead of manually managing HTTP status code and request-response models. It leverages a **Result Pattern** to streamline operation outcomes and take advantage of the latest improvements in **.NET Core 8**.

https://devblogs.microsoft.com/odata/build-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice/
---
This package supports the full OData syntax and is extended to also provide data types and ETag attributes.

On top of the OData support, out of the box a documentation and test UI is added out of the box. It looks a lot like the UI for the Microsoft Graph API explorer and even shows implementation examples in the coding language of your choice.

Finally we have included the optional authentication and authorization features for Microsoft Entra for both the usage of the UI and the actual Api's themselves. Which can be configured independent from each other.

---

### Step 1 Installation
Using .NET CLI:
```bash
dotnet add package Mediatr.OData.Api
```

Using Package Manager:
```bash
Install-Package Mediatr.OData.Api
```

---

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

---

### Step 3 Add the configuration for the OData API configuration to the appsettings.json
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

---

### Step 4 (optional) Add the configuration for the Entra authentication and authorization to your appsettings.json
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

---

### Step 5 Create a DomainModel
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

---

### Step 6 Create a GET handler implementation to actually have 2 endpoints
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

---

### Step 7 Create a POST handler implementation
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

---

### Step 8 Create a PATCH or PUT handler implementation
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

---

### Step 9 Create a Delete handler implementation
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

---

### Step 10 GET Navigation handler on related objects from within the same group
Something that OData does not standard support but what makes the usage more flexibel is the child object navigation.
Think of the following scenario:
You have a Department and inside a department there are Employees and you just fetched a specific department through
the endpoint:
```curl
https://somedomain/odata/departments/{key}
```
And now you would like to have in a new call all employees that belong to this department.
In a normal OData restfull world you would do the following:
```curl
https://somedomain/odata/employees?filter=departmentId eq {key}
```
This means you need to be aware of the foreign key name and create a filter.
We included a nicer way (just like the Microsoft Graph API explorer) to do just this by supporting the following:
```curl
https://somedomain/odata/departments/{key}/employees
```
As you can see this syntax is much simpler for the end-user of the API and does not require a lot of work at all.
All you need to do is to include a navigationtype endpoint in the same group.
See the implementation for the above example:

```C#
    [Endpoint<Department, int, Employee>(EndpointMethod.Get, "employees")]
    public class GetEmployeesHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Department, int, Employee>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Employee> options, CancellationToken cancellationToken)
        {
            var data = await GetEmployeesFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }
```
As you can see the implementation is very simple and If you plan your data access strategy smart, you will not have to write any special code to support the OData syntax on the result which gives you the same functionality as you would have when you would call the GET employees endpoint directly. But this one makes much more sense. It also means that you could decide not to include endpoints for domainobjects which you don't want to expose directly (for example when it does not require a post/put/patch/delete endpoint.) 

This method does not only work when there is a one to many relationship but also with one to one relationships.
In our example this is Company. 
See the implementation for this scenario:
```C#
    [Endpoint<Department, int, Company>(EndpointMethod.Get, "company")]
    public class GetCompanyHandler(IDbConnection connection) : IEndpoinGetByNavigationHandler<Department, int, Company>
    {
        public async Task<Result<dynamic>> Handle(int key, Type TDomainObject, ODataQueryOptionsWithPageSize<Company> options, CancellationToken cancellationToken)
        {
            var data = await GetCompanyFromDB(connection, key);
            return options.ApplyODataOptions(data);
        }
    }
}
```
**Important note** : They key in the above 2 examples are actually the key of the Department you get supplied so you will have to navigate through your domain model based on that key.
```C#
   //Populate your domain model based on the department 
   var department = GetDepartmentFromDB(key);
   var employees = department.Employees.ToList();
   var company = department.Company;
```
---

### Still to describe is the Authorization usage

---

### Still to describe is the detailed explanation of appsettings.json

---

### Still to describe is the detailed explanation of the Atrributes usage

---

### Still to descibe is the special functions to make Boolean, Values, Enumerations, DateTime functions more versatile compared to the default behaviour of any JSON serializere/deserializer

---

### Still to document is the OData Syntax 
working on it
---

### Still to do is the work in progress (enhancements) that are on the roadmap
- Post/Put/Patch also support OData syntax (for select, expand etc, so you can omit values you don't need)
- Multikeynavigation Department/{key}/Employees/{key}/Department/Company (for example)
- Multilevel Patch/Put and Post new Department including Employees or new Employees
- Other Authentication and Authorization implementations by the usage of a SecurityProvider instead of Entra only

---

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

### **Authentication and Authorization**

The framework integrates seamlessly with ASP.NET Core's **authentication** and **authorization** mechanisms to provide secure access to entities and operations. This includes:

1. **Authentication**: Ensures that only authenticated users can access the application.
2. **Authorization**: Enforces role-based and method-specific access control, leveraging:
   - **`EntityAuthorize`**: Custom attribute for CRUD operations, inheriting from ASP.NET Core's `AuthorizeAttribute`.
   - **ASP.NET Core's `AuthorizeAttribute`**: Used for custom entity operations.

By following the ASP.NET Core model, the framework ensures compatibility with existing security configurations, such as `JwtBearer`, `CookieAuthentication`, or external providers (e.g., OAuth, OpenID Connect).

### Example: Configuring Authorization for an Domainobject
```csharp
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.Query })] // Only authorized users can query the entity.
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.GetByKey }, Roles = TestUtils.AdminRole)] // Admin-only access for GetByKey.
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.Post }, Roles = $"{TestUtils.AdminRole},{TestUtils.SupperAdminRole}")] // Admin and SuperAdmin access for Post.
[EntityAuthorize(ApplyMethods = new[] { EntityMethod.Delete }, Roles = TestUtils.SupperAdminRole)] // SuperAdmin-only access for Delete.
public class MultiAuthorization : IDomainObject<Guid>
{
    public Guid Id { get; set; }
}
```
---

