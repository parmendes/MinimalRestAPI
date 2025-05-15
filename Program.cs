using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;

var builder = WebApplication.CreateBuilder(args);

// ==================== Services Configuration ====================
// Configuration organized into extension methods for better readability and maintainability
builder.Services.AddApiVersioningSupport(); // Configure API versioning and API explorer for Swagger documentation
builder.Services.AddSwaggerGen(); // Add Swagger generator to the service collection
builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>(); // Configure SwaggerGen options
builder.Services.AddAuthenticationAndAuthorization(); // Configure authentication and authorization services
builder.Services.AddCustomIdentityServer(); // Configure IdentityServer with in-memory clients, API scopes, and identity resources
builder.Services.AddCorsPolicy(); // Configure CORS policy to allow requests from the Swagger UI
builder.Services.AddRateLimiting(builder.Configuration); // Configure rate limiting for the API

// ==================== Application Configuration ====================
var app = builder.Build();

// Enable rate limiting middleware
app.UseRateLimiter(); 

// Define the API version set
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(1.0)
    .HasApiVersion(2.0)
    .ReportApiVersions() // Report API versions in the response headers
    .Build();

// Define the route group for versioned endpoints
RouteGroupBuilder versionedGroup = app.MapGroup("/api/v{version:apiVersion}/weatherforecast")
            .WithApiVersionSet(versionSet)
            //.RequireRateLimiting("weatherforecast") // Apply rate limiting policy to all endpoints. It is superseded by policies applied in the endpoints.
            .WithTags("WeatherForecast") // Tag for grouping in Swagger UI
            ;

// Map weather forecast endpoints to the route group
versionedGroup.MapWeatherForecastEndpoints(); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        IReadOnlyList<ApiVersionDescription> description = app.DescribeApiVersions();

        foreach (ApiVersionDescription desc in description)
        {
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", string.Concat(desc.GroupName.ToUpperInvariant(), "(JSON)"));
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.yaml", string.Concat(desc.GroupName.ToUpperInvariant(), "(YAML)"));
        }
        
        // c.OAuthClientId("minimalrestapi-client");
        // c.OAuthClientSecret("your-client-secret");
        // c.OAuthUsePkce(); // opcional para client credentials
    });
}

//app.UseHttpsRedirection(); // Enable HTTPS redirection
app.UseCors("AllowSwaggerUI"); // Apply the CORS policy
//app.UseIdentityServer(); // Enable IdentityServer middleware	
//app.UseAuthentication(); // Enable authentication middleware
//app.UseAuthorization(); // Enable authorization middleware



app.Run();