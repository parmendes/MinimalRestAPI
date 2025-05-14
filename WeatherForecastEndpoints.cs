using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

/// <summary>
/// Provides extension methods to map WeatherForecast-related endpoints.
/// </summary>
public static class WeatherForecastEndpoints
{
    /// <summary>
    /// Maps the WeatherForecast endpoints to the specified endpoint route builder.
    /// </summary>
    /// <param name="app">The route builder used to register the endpoints.</param>
    public static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder app)
    {
        // Map the public endpoints (unauthenticated)
        // The endpoints are accessible without authentication and are open to all users
        MapPublicEndpoints(app);

        // Map the authenticated endpoints (Admin/User roles)
        // The endpoints are protected by the [Authorize] attribute and require authentication
        MapAuthenticatedEndpoints(app);
    }

    #region Public Endpoints

    /// <summary>
    /// Maps all public (unauthenticated) endpoints.
    /// </summary>
    private static void MapPublicEndpoints(IEndpointRouteBuilder app)
    {
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        // Versioned GET endpoint for 5-day forecast (v1 and v2)
        app.MapGet("", [AllowAnonymous] (
            [FromHeader(Name = "X-My-Custom-Header")] string? myHeaderValue,
            [FromQuery, DefaultValue(-20)] int minTemperature,
            [FromQuery, DefaultValue(55)] int maxTemperature) =>
        {
            if (minTemperature >= maxTemperature)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid temperature range",
                    Detail = "'minTemperature' must be less than 'maxTemperature'.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(minTemperature, maxTemperature),
                    summaries[Random.Shared.Next(summaries.Length)]
                )).ToArray();

            return Results.Ok(forecast);
        })
        .WithName("GetWeatherForecast")
        .MapToApiVersion(1.0)
        .MapToApiVersion(2.0)
        .RequireRateLimiting("FivePerMinute") // Apply rate limiting policy
        .Produces<WeatherForecast[]>(200)
        .Produces(400, typeof(ProblemDetails))
        .Produces(429, typeof(ProblemDetails))
        .Produces(500, typeof(ProblemDetails))
        .WithOpenApi(operation =>
        {
            operation.Summary = "Retrieves the weather forecast for the next 5 days.";
            operation.Description = "Returns a list of weather forecasts with temperatures and summaries.";
            operation.Responses["200"].Description = "Successful operation.";
            operation.Responses["400"].Description = "Invalid temperature range.";
            operation.Responses["429"].Description = "API calls quota exceeded!";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });

        // New 3-day forecast only for version 2
        app.MapGet("/3Days", [AllowAnonymous] () =>
        {
            var shortSummaries = new[] { "Cold", "Warm", "Hot" };

            var forecast = Enumerable.Range(1, 3).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-10, 30),
                    shortSummaries[Random.Shared.Next(shortSummaries.Length)]
                )).ToArray();

            return Results.Ok(forecast);
        })
        .WithName("GetWeatherForecast3Days")
        .MapToApiVersion(2.0)
        .Produces<WeatherForecast[]>(200)
        .Produces(429, typeof(ProblemDetails))
        .Produces(500, typeof(ProblemDetails))
        .WithOpenApi(operation =>
        {
            operation.Summary = "Retrieves a short-term 3-day weather forecast.";
            operation.Description = "Newer version with limited 3-day forecast data.";
            operation.Responses["200"].Description = "Successful operation.";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });

        // Legacy endpoint (Deprecated)
        app.MapGet("/legacy", [AllowAnonymous] () =>
            Results.Ok("This is a legacy endpoint.")
        )
        .WithName("GetLegacyWeatherForecast")
        .MapToApiVersion(1.0)
        .WithOpenApi(operation =>
        {
            operation.Deprecated = true;
            operation.Summary = "Legacy endpoint (deprecated).";
            operation.Description = "This endpoint is deprecated. Use '/weatherforecast' instead.";
            return operation;
        });
    }

    #endregion

    #region Authenticated Endpoints

    /// <summary>
    /// Maps all authenticated endpoints (Admin/User roles).
    /// </summary>
    private static void MapAuthenticatedEndpoints(IEndpointRouteBuilder app)
    {
        // Admin-only POST
        app.MapPost("/admin-endpoint", [Authorize(Policy = "AdminOnly")] (WeatherForecast forecast) =>
            Results.Created($"/admin-endpoint/{forecast.Date}", forecast)
        )
        .WithName("CreateWeatherForecastADMIN")
        .MapToApiVersion(1.0)
        .MapToApiVersion(2.0)
        .Produces<WeatherForecast>(201)
        .Produces(400, typeof(ProblemDetails))
        .Produces(401, typeof(ProblemDetails))
        .Produces(429, typeof(ProblemDetails))
        .Produces(500, typeof(ProblemDetails))
        .WithOpenApi(operation =>
        {
            operation.Summary = "Creates a new weather forecast (Admin only).";
            operation.Description = "Only users with Admin privileges can access this endpoint.";
            operation.Responses["201"].Description = "Created successfully.";
            operation.Responses["400"].Description = "Invalid input.";
            operation.Responses["401"].Description = "Unauthorized access.";
            operation.Responses["500"].Description = "Server error.";
            return operation;
        });

        // General authenticated POST
        app.MapPost("/user-endpoint", [Authorize(Policy = "ApiScope")] (WeatherForecast forecast) =>
            Results.Created($"/user-endpoint/{forecast.Date}", forecast)
        )
        .WithName("CreateWeatherForecast")
        .MapToApiVersion(1.0)
        .MapToApiVersion(2.0)
        .Produces<WeatherForecast>(201)
        .Produces(400, typeof(ProblemDetails))
        .Produces(401, typeof(ProblemDetails))
        .Produces(429, typeof(ProblemDetails))
        .Produces(500, typeof(ProblemDetails))
        .WithOpenApi(operation =>
        {
            operation.Summary = "Creates a new weather forecast.";
            operation.Description = "Authenticated users can create forecasts.";
            operation.Responses["201"].Description = "Created.";
            operation.Responses["400"].Description = "Invalid input.";
            operation.Responses["401"].Description = "Unauthorized.";
            operation.Responses["500"].Description = "Server error.";
            return operation;
        });

        // PUT (update) endpoint
        app.MapPut("/{date}", [Authorize(Policy = "ApiScope")] (WeatherForecast updatedForecast) =>
            Results.Ok(updatedForecast)
        )
        .WithName("UpdateWeatherForecast")
        .MapToApiVersion(2.0)
        .Produces<WeatherForecast>(200)
        .Produces(400, typeof(ProblemDetails))
        .Produces(401, typeof(ProblemDetails))
        .Produces(404, typeof(ProblemDetails))
        .Produces(429, typeof(ProblemDetails))
        .Produces(500, typeof(ProblemDetails))
        .WithOpenApi(operation =>
        {
            operation.Summary = "Updates an existing forecast.";
            operation.Description = "Allows updates to a forecast by date.";
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "date",
                In = Microsoft.OpenApi.Models.ParameterLocation.Path,
                Required = true,
                Description = "Date of the forecast. Format: yyyy-MM-dd.",
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "date",
                    Pattern = @"^\d{4}-\d{2}-\d{2}$"
                }
            });

            /* operation.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.Schema,
                                Id = "WeatherForecast"
                            }
                        },
                        // Using this example will override what is defined in the WeatherForecast when showing the the Example of the request body.
                        // The schema, however, continues to show what is in the model.
                        Example = new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["date"] = new Microsoft.OpenApi.Any.OpenApiString("2025-04-24"),
                            ["temperatureC"] = new Microsoft.OpenApi.Any.OpenApiInteger(30),
                            ["summary"] = new Microsoft.OpenApi.Any.OpenApiString("Partly Cloudy")
                        }
                    }
                }
            }; */

            operation.Responses["200"].Description = "Updated.";
            operation.Responses["400"].Description = "Bad request.";
            operation.Responses["401"].Description = "Unauthorized.";
            operation.Responses["404"].Description = "Not found.";
            operation.Responses["429"].Description = "API calls quota exceeded!";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });

        // DELETE endpoint
        app.MapDelete("/{date}", [Authorize(Policy = "ApiScope")] () =>
            Results.NoContent()
        )
        .WithName("DeleteWeatherForecast")
        .MapToApiVersion(2.0)
        .Produces(204)
        .Produces(400, typeof(ProblemDetails))
        .Produces(401, typeof(ProblemDetails))
        .Produces(404, typeof(ProblemDetails))
        .Produces(429, typeof(ProblemDetails))
        .Produces(500, typeof(ProblemDetails))
        .WithOpenApi(operation =>
        {
            operation.Summary = "Deletes a forecast.";
            operation.Description = "Removes a forecast by date.";
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "date",
                In = Microsoft.OpenApi.Models.ParameterLocation.Path,
                Required = true,
                Description = "Date of forecast to delete. Format: yyyy-MM-dd.",
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "date",
                    Pattern = @"^\d{4}-\d{2}-\d{2}$"
                }
            });

            operation.Responses["204"].Description = "Deleted.";
            operation.Responses["400"].Description = "Bad request.";
            operation.Responses["401"].Description = "Unauthorized.";
            operation.Responses["404"].Description = "Not found.";
            operation.Responses["429"].Description = "API calls quota exceeded!";
            operation.Responses["500"].Description = "Server error.";
            return operation;
        });
    }

    #endregion
}

/// <summary>
/// Represents a weather forecast record.
/// </summary>
/// <param name="date">The date of the forecast.</param>
/// <param name="temperatureC">Temperature in Celsius.</param>
/// <param name="summary">A short description of the weather.</param>
public class WeatherForecast(DateOnly date, int temperatureC, string? summary)
{
    /// <summary>
    /// Gets the date of the weather forecast.
    /// </summary>
    public DateOnly Date { get; } = date;

    /// <summary>
    /// Gets the temperature in Celsius.
    /// </summary>
    [Range(-50, 50)]
    [DefaultValue(15)]
    public int TemperatureC { get; } = temperatureC;

    /// <summary>
    /// Gets the temperature in Fahrenheit.
    /// </summary>
    public int TemperatureF { get; } = 32 + (int)(temperatureC / 0.5556);

    /// <summary>
    /// Gets the summary of the forecast.
    /// </summary>
    [StringLength(100)]
    [DefaultValue("Default summary")]
    public string? Summary { get; } = summary;
}


