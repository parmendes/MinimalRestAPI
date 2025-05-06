using Asp.Versioning;

/// <summary>
/// This class contains the configuration for API versioning in the application.
/// It sets up the default API version and enables reporting of API versions.
/// Also configures the API Explorer to support versioned Swagger documentation.
/// </summary>
public static class ApiVersioningConfiguration
{
    /// <summary>
    /// Adds API versioning and API explorer support to the service collection.
    /// This ensures that the API can support versioned endpoints and that Swagger/OpenAPI
    /// documentation can reflect each versioned group accordingly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public static void AddApiVersioningSupport(this IServiceCollection services)
    {
        // Configure basic API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0); // Set the default API version to 1.0
            options.AssumeDefaultVersionWhenUnspecified = true; // Use the default version if none is specified
            options.ReportApiVersions = true; // Include API version information in the response headers      
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader() // Read API version from the URL segment
            ); 
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // Format for the API version in the group name
            options.SubstituteApiVersionInUrl = true; // Substitute the API version in the URL
        });

        // This method adds support for API versioning and API explorer to the service collection.
        services.AddEndpointsApiExplorer(); 
    }
}
