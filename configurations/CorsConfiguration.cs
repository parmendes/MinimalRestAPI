/// <summary>
/// This class contains extension methods for configuring CORS (Cross-Origin Resource Sharing) in the application.
/// It sets up a CORS policy that allows requests from the Swagger UI to access the API.
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Adds CORS policy to the application.
    /// This method configures a CORS policy that allows requests from the Swagger UI to access the API.
    /// </summary>
    /// <param name="services"></param>
    public static void AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSwaggerUI", policy =>
            {
                policy.WithOrigins("http://localhost:5009")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Allow credentials for token requests
            });
        });
    }
}