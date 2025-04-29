using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================== Services Configuration ====================

// Add Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>  
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MinimalRestAPI",
        Version = "v1",
        Description =         "### MinimalRestAPI\n\n" +
        "An internal API for managing weather forecasts. This API is not intended for external use and is rate-limited to 100 requests per minute for free users.\n\n" +
        "#### Features:\n" +
        "- Retrieve weather forecasts for the next 5 days.\n" +
        "- Create new weather forecasts.\n" +
        "- Update existing weather forecasts.\n" +
        "- Delete weather forecasts.\n\n" +
        "#### Notes:\n" +
        "- Authentication is required for most operations.\n" +
        "- Ensure proper usage of the API within the defined rate limits."
    });

    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
        Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
        {
            ClientCredentials = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
            {
                TokenUrl = new Uri("https://localhost:5001/connect/token"), // IdentityServer token endpoint
                Scopes = new Dictionary<string, string>
                {
                    { "api1", "Access MinimalRestAPI" }
                }
            }
        }
    });

    options.MapType<ProblemDetails>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSchema>
        {
            ["type"] = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" },
            ["title"] = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" },
            ["status"] = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "integer", Format = "int32" },
            ["detail"] = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" },
            ["instance"] = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
        }
    });

    // Add the security requirement for the API
    // This will require the client to provide a token for the "api1" scope
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "api1" }
        }
    });

    // Add the AuthorizeCheckOperationFilter to check for authorization attributes on endpoints
    // This will ensure that the Swagger UI shows the authorization button for endpoints that require authentication
    options.OperationFilter<AuthorizeCheckOperationFilter>();    

    // Generate YAML output
    options.UseAllOfToExtendReferenceSchemas();
});

// Add services to the container.
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // Seu IdentityServer
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

// Add authorization policies
// This policy requires the user to be authenticated and have the "api1" scope
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1"); // 
    });
});

// Add IdentityServer configuration
// This configures IdentityServer with in-memory clients, API scopes, and identity resources
builder.Services.AddIdentityServer()
    .AddInMemoryClients(new List<Client>
    {
        new Client
        {
            ClientId = "minimalrestapi-client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets =
            {
                new Secret("your-client-secret".Sha256())
            },
            AllowedScopes = { "api1" }
        }
    })
    .AddInMemoryApiScopes(new List<ApiScope>
    {
        new ApiScope("api1", "Access MinimalRestAPI")
    })
    .AddInMemoryIdentityResources(new List<IdentityResource>
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    })
    .AddDeveloperSigningCredential(); // For development only

// Configure CORS policy for Swagger UI
// This allows the Swagger UI to access the API from a different origin (e.g., localhost:5009)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwaggerUI", policy =>
    {
        policy.WithOrigins("http://localhost:5009") // Allow Swagger UI origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Allow credentials for token requests
    });
});

// ==================== Application Configuration ====================

var app = builder.Build();

// Log incoming requests for debugging
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"{header.Key}: {header.Value}");
    }
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "MinimalRestAPI v1 (YAML)"); //Serve the YAML file.
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MinimalRestAPI v1 (JSON)");
    c.OAuthClientId("minimalrestapi-client");
    c.OAuthClientSecret("your-client-secret");
    c.OAuthUsePkce(); // opcional para client credentials
});


app.UseHttpsRedirection(); // Enable HTTPS redirection
app.UseCors("AllowSwaggerUI"); // Apply the CORS policy
app.UseIdentityServer(); // Enable IdentityServer middleware	
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware
app.MapWeatherForecastEndpoints(); // Register the WeatherForecast endpoints

app.Run();