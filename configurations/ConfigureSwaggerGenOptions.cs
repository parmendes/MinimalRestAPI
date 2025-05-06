
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// This class is responsible for configuring SwaggerGen options for the API documentation.
/// It sets up the Swagger documentation for each API version and provides metadata for the API.
/// </summary>
public class ConfigureSwaggerGenOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerGenOptions"/> class.
    /// This constructor is used to inject the API version description provider into the class.
    /// </summary>
    /// <param name="provider">The API version description provider.</param>
    public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Configures the SwaggerGen options.
    /// This method is called when the SwaggerGen options are registered in the service collection.
    /// </summary>
    /// <param name="options">The SwaggerGen options to configure.</param>
    public void Configure(SwaggerGenOptions options)
    {
        // Add a Swagger document for each API version
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        // Add security definition for OAuth2
            // This will allow the Swagger UI to show the authorization button for endpoints that require authentication
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    ClientCredentials = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri("https://localhost:5001/connect/token"), // IdentityServer token endpoint
                        Scopes = new Dictionary<string, string>
                        {
                            { "api1", "Access MinimalRestAPI" }
                        }
                    }
                }
            });

            // Map ProblemDetails to OpenAPI schema
            // This will ensure that the ProblemDetails object is correctly represented in the Swagger UI
            options.MapType<ProblemDetails>(() => new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["type"] = new OpenApiSchema { Type = "string" },
                    ["title"] = new OpenApiSchema { Type = "string" },
                    ["status"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                    ["detail"] = new OpenApiSchema { Type = "string" },
                    ["instance"] = new OpenApiSchema { Type = "string" }
                }
            });

            // Add the security requirement for the API
            // This will require the client to provide a token for the "api1" scope
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "api1" }
                }
            });

            // Add the AuthorizeCheckOperationFilter to check for authorization attributes on endpoints
            // This will ensure that the Swagger UI shows the authorization button for endpoints that require authentication
            options.OperationFilter<AuthorizeCheckOperationFilter>();    

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = $"MinimalRestAPI_{description.ApiVersion}",
            Version = description.ApiVersion.ToString(),
            Description = "### MinimalRestAPI\n\n" +
                "An internal API for managing weather forecasts. This API is not intended for external use and is rate-limited to 100 requests per minute for free users.\n\n" +
                "#### Notes:\n" +
                "- Authentication is required for most operations.\n" +
                "- Ensure proper usage of the API within the defined rate limits.",
            Contact = new OpenApiContact() { Name = "MinimalRestAPI", Email = string.Empty },
            License = new OpenApiLicense() { Name = "Use under MIT", Url = null }
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }

    /// <summary>
    /// Configures the SwaggerGen options with a specific name.
    /// This method is used when the options are registered with a name in the service collection.
    /// </summary>
    /// <param name="name"> The name of the options to configure.</param>
    /// <param name="options"> The SwaggerGen options to configure.</param>
    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options); // Call the main configuration method
    }
}