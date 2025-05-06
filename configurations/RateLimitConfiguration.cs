using AspNetCoreRateLimit;

/// <summary>
/// This class contains methods to configure rate limiting for the API.
/// It sets up a rate limit policy that restricts the number of requests from a single client.
/// </summary>
public static class AddRateLimitConfiguration
{
    /// <summary>
    /// Adds rate limiting to the application.
    /// This method configures a rate limit policy that restricts the number of requests from a single client.
    /// </summary>
    /// <param name="services"> The service collection to configure.</param>
    /// <param name="configuration"> The configuration to use for rate limiting.</param>
    public static void AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();
    }
}