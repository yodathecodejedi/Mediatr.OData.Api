using CFW.ODataCore.SchemaFilters;
using Mediatr.OData.Api.Factories;
using Mediatr.OData.Api.Models;
using Mediatr.OData.Api.SchemaFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Mediatr.OData.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void CreateAndRegisterODataRoutes(this WebApplicationBuilder? builder, string ConfigurationSection = default!)
    {
        if (builder is null) return;

        if (ConfigurationSection.IsNullOrWhiteSpace())
            ConfigurationSection = "OData";

        var configuration = builder.Configuration.GetSection(ConfigurationSection).Get<ODataConfiguration>() ?? new ODataConfiguration();
        AppContext.SetData("ODataConfiguration", configuration);

        //Configure the HTTP Options
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.SerializerOptions.Converters.Add(new IgnoreSpecializedPropertiesPolicy());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
        });

        if (configuration.SecureAPI)
            builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "Entra");
        if (configuration.SecureWebInterface)
            builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "Entra");
        if (configuration.SecureAPI || configuration.SecureWebInterface)
            builder.Services.AddAuthorization();


        //This is our OData 
        builder.Services.AddEndpointsApiExplorer();

        //Because of Scalar / Swagger UI
        builder.Services.AddSwaggerGen(options =>
        {
            options.OperationFilter<CountSchemaFilter>();
            options.SchemaFilter<ExcludePropertiesSchemaFilter>();
            options.SchemaFilter<EnumSchemaFilter>();
            if (configuration.SecureAPI)
            {

                options.AddSecurityDefinition("Bearer Auth", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Input bearer token to access this API",
                });
            }
        }
        );

        //This is OUR oData 
        builder.Services.AddODataEndpoints(configuration.RoutePrefix);
    }
}
