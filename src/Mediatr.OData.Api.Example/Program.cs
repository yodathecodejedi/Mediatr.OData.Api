using Mediatr.OData.Api.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IDbConnection>(connection => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.CreateAndRegisterODataRoutes();

var app = builder.Build();

app.AttachODataRoutes();

app.Run();