using Microsoft.IdentityModel.Tokens;

/// <summary>
/// This class contains extension methods for configuring authentication and authorization in the application.
/// It sets up JWT Bearer authentication and defines authorization policies for different roles and scopes.
/// </summary>
public static class AuthConfiguration
{
    /// <summary>
    /// Adds authentication and authorization services to the application.
    /// This method configures JWT Bearer authentication and sets up authorization policies.
    /// </summary>
    /// <param name="services"></param>
    public static void AddAuthenticationAndAuthorization(this IServiceCollection services)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "https://localhost:5001";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("role", "admin");
                policy.RequireRole("admin");
            });

            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "api1");
            });
        });
    }
}
