using Mediatr.OData.Api.Extensions;
using Mediatr.OData.Exampl.DomainRepository;
using Mediatr.OData.Exampl.DomainRepository.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IDbConnection>(x => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<IRepository, Repository>();
builder.CreateAndRegisterODataRoutes();

var app = builder.Build();

app.AttachODataRoutes();

app.Run();