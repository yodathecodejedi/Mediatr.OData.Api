# Mediatr OData API

**Mediatr OData API** is a lightweight framework built on **ASP.NET Core OData** and **Minimal API**, fully compatible with **.NET Core 8**. It is designed to reduce the development time of API's drasticly and simplify object and entity management by focusing on clean, extensible handlers. Instead of manually managing HTTP status codes and request-response models, it leverages a **Result Pattern** to streamline operation outcomes and take advantage of the latest improvements in **.NET Core 8**.
<https://devblogs.microsoft.com/odata/build-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice/>
---

### Installation

Using .NET CLI:

```bash
dotnet add package Mediatr.OData.Api
```

Using Package Manager:

```bash
Install-Package Mediatr.OData.Api
```

In the ASP.NET Core (Mimimal) API project, update your `Program.cs` as below:

```C#
using Mediatr.OData.Api.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

//Add a transient service to any data source for injection if you need it in your project (seems a given) but use one to your own liking/requirement.
builder.Services.AddTransient<IDbConnection>(x => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

//This is where the magic happens
builder.CreateAndRegisterODataRoutes();

var app = builder.Build();

//This is where all registered handlers (endpoints) are attached and the UI for the API is enabled
app.AttachODataRoutes();

app.Run();
```

In the above code you only need 2 lines of code to make things work (`builder.CreateAndRegisterODataRoutes()`, and `app.AttachODataRoutes()`)

Create a new ViewModel or DbModel class and add the `[Entity]` attribute to it. The framework will automatically generate CRUD endpoints for the entity.:

```C#
TODO
```

---

## Key Features

- [Mediatr OData API](#mediatr-odata-api)
  - [https://devblogs.microsoft.com/odata/build-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice/](#httpsdevblogsmicrosoftcomodatabuild-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice)
    - [Installation](#installation)
  - [Key Features](#key-features)
      - [GET: Query and get by key with OData query options](#get-query-and-get-by-key-with-odata-query-options)
      - [POST `/products` with 1-Level nested relationships](#post-products-with-1-level-nested-relationships)
      - [PATCH `/products/{id}` with partial updates](#patch-productsid-with-partial-updates)
      - [DELETE `/products/{id}`](#delete-productsid)
    - [**4. Authentication and Authorization**](#4-authentication-and-authorization)
    - [Example: Configuring Authorization for an Entity](#example-configuring-authorization-for-an-entity)

#### GET: Query and get by key with OData query options

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

#### POST `/products` with 1-Level nested relationships

The framework supports creating:

1. **Main Entity**: Insert a single entity into the database.
2. **Child Entities (1 Level Nested)**: Handle child entities (either a single complex entity or a collection of entities) nested directly under the main entity.  
   - **Existing Child Entities**: If the child item's ID exists in the database, it is ignored.
   - **New Child Entities**: If the child item's ID is null or not in the database, it is inserted.

| Scenario                          | Request Body                                                                                                                                         | Explanation                                                                                                                                                          |
|-----------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Main Entity Only**              | ```{ "name": "Product A", "price": 150.0 }```                                                                                                  | Inserts the `Product` entity without any nested relationships.                                                                                                       |
| **Main Entity with Single Child** | ```{ "name": "Product B", "price": 120.0, "category": { "name": "Electronics" } }```                                                            | Inserts the `Product` entity and the `Category` entity if it doesn't exist. If the `Category` exists, it is associated with the `Product`.                          |
| **Main Entity with Child List**   | ```{ "name": "Product C", "price": 200.0, "orders": [ { "id": 1, "quantity": 2 }, { "quantity": 3 } ] }```                                      | Inserts the `Product` entity. Existing `Orders` (with `id`) are associated, and new `Orders` (without `id`) are inserted and associated.                            |

---

#### PATCH `/products/{id}` with partial updates

The PATCH method supports updating specific fields of an existing <strong>ROOT</strong> entity. The request body should contain only the fields that need to be updated.

| Scenario              | Request Body                                                                                         | SQL Generated                                                                                   | Explanation                                                                                  |
|-----------------------|-----------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| **Update Name Only**  | ```json { "name": "Updated Product A" }```                                                         | ```sql UPDATE Products SET Name = 'Updated Product A' WHERE Id = 1;```                        | Updates the `name` of the `Product` entity while leaving other fields unchanged.            |
| **Update Price Only** | ```json { "price": 175.0 }```                                                                      | ```sql UPDATE Products SET Price = 175.0 WHERE Id = 2;```                                     | Updates the `price` of the `Product` entity while leaving other fields unchanged.           |
| **Update Multiple**   | ```json { "name": "Updated Product B", "price": 200.0 }```                                         | ```sql UPDATE Products SET Name = 'Updated Product B', Price = 200.0 WHERE Id = 3;```        | Updates both `name` and `price` fields of the `Product` entity while leaving others intact. |

---

#### DELETE `/products/{id}`

| Scenario              | SQL Generated                                                   | Explanation                                                                                  |
|-----------------------|-----------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| **Delete by ID**      | ```sql DELETE FROM Products WHERE Id = 1;```                   | Deletes the `Product` entity with ID `1`.                                                   |

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
