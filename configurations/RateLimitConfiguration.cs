using System.Threading.RateLimiting;

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
        // Get the rate limiting configuration section from appsettings.json
        var rateLimitingConfig = configuration.GetSection("RateLimiting"); 

        services.AddRateLimiter(options =>
        {
            // Set the status code for rejected requests to 429 (Too Many Requests), instead of 503 (Service Unavailable)
            // This means that when a request exceeds the rate limit, it will return a 429 status code
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Set the maximum number of concurrent requests allowed
            // This is the maximum number of requests that can be processed at the same time
            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var raw))
                {
                    if (raw is TimeSpan delay)
                    {
                        // Set the Retry-After header in the response to indicate when the client can retry the request
                        context.HttpContext.Response.Headers.RetryAfter = ((int)delay.TotalSeconds).ToString();
                    }
                }

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                "{\"error\":\"Too many requests. Try again later.\"}", ct);
            };
            
            // Define a global rate limiter that applies to all requests
            // This is a no-op limiter, meaning it doesn't actually limit requests
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetNoLimiter<string>("global"));

            // Define a rate limit policy for the "weatherforecast" endpoint
            // This policy allows 5 requests every minute
            // The partition key is based on the client's IP address
            options.AddPolicy("FivePerMinute", context =>
                RateLimitPartition.GetFixedWindowLimiter<string>(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }
                )
            );

            // Define a rate limit policy for the "weatherforecast" endpoint
            // This policy allows 10 requests every 30 seconds
            options.AddPolicy("TenPer30Seconds", context =>
                RateLimitPartition.GetFixedWindowLimiter<string>(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }
                )
            );
        });
    }
}