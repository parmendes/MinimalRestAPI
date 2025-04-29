using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Provides extension methods to map WeatherForecast-related endpoints.
/// </summary>
public static class WeatherForecastEndpoints
{
    /// <summary>
    /// Maps the WeatherForecast endpoints to the specified endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the endpoints to.</param>
    public static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        // GET endpoint (public, no authentication required)
        endpoints.MapGet("/weatherforecast", [AllowAnonymous] () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .Produces<WeatherForecast[]>(200) // Response schema
        .Produces(400, typeof(ProblemDetails)) // Bad request
        .Produces(500, typeof(ProblemDetails)) // Internal server error
        .WithOpenApi(operation =>
        {
            operation.Summary = "Retrieves the weather forecast for the next 5 days.";
            operation.Description = "This endpoint provides a list of weather forecasts for the next 5 days, including temperature and summary.";
            operation.Responses["200"].Description = "A list of weather forecasts.";
            operation.Responses["400"].Description = "Bad request. The input parameters are invalid.";
            operation.Responses["500"].Description = "Internal server error. Something went wrong on the server.";
            return operation;
        });

        // GET legacy endpoint (public, no authentication required)
        endpoints.MapGet("/weatherforecast/legacy", [AllowAnonymous] () =>
        {
            return Results.Ok("This is a legacy endpoint.");
        })
        .WithName("GetLegacyWeatherForecast")
        .WithOpenApi(operation =>
        {
            operation.Deprecated = true;
            operation.Description = "This endpoint is deprecated. Use /weatherforecast instead.";
            return operation;
        });

        // POST endpoint (requires authentication)
        endpoints.MapPost("/weatherforecast", [Authorize(Policy = "ApiScope")] (WeatherForecast forecast) =>
        {
            return Results.Created($"/weatherforecast/{forecast.Date}", forecast);
        })
        .WithName("CreateWeatherForecast")
        .Produces<WeatherForecast>(201) // Response schema
        .Produces(400, typeof(ProblemDetails)) // Bad request
        .Produces(401, typeof(ProblemDetails)) // Unauthorized
        .Produces(500, typeof(ProblemDetails)) // Internal server error
        .WithOpenApi(operation =>
        {
            operation.Summary = "Creates a new weather forecast.";
            operation.Description = "This endpoint allows authenticated users to create a new weather forecast.";
            operation.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody
            {
                Content = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType>
                {
                    ["application/json"] = new Microsoft.OpenApi.Models.OpenApiMediaType
                    {
                        Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.Schema,
                                Id = "WeatherForecast"
                            }
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["date"] = new Microsoft.OpenApi.Any.OpenApiString("2025-04-24"),
                            ["temperatureC"] = new Microsoft.OpenApi.Any.OpenApiInteger(25),
                            ["summary"] = new Microsoft.OpenApi.Any.OpenApiString("Sunny")
                        }
                    }
                }
            };
            operation.Responses["201"].Description = "Weather forecast created successfully.";
            operation.Responses["400"].Description = "Bad request. The input parameters are invalid.";
            operation.Responses["401"].Description = "Unauthorized. The user is not authenticated.";
            operation.Responses["500"].Description = "Internal server error. Something went wrong on the server.";

            return operation;
        });

        // PUT endpoint (requires authentication)
        endpoints.MapPut("/weatherforecast/{date}", [Authorize(Policy = "ApiScope")] (WeatherForecast updatedForecast) =>
        {
            return Results.Ok(updatedForecast);
        })
        .WithName("UpdateWeatherForecast")
        .Produces<WeatherForecast>(200) // Response schema
        .Produces(400, typeof(ProblemDetails)) // Bad request
        .Produces(401, typeof(ProblemDetails)) // Unauthorized
        .Produces(404, typeof(ProblemDetails)) // Not found
        .Produces(500, typeof(ProblemDetails)) // Internal server error
        .WithOpenApi(operation =>
        {
            operation.Summary = "Updates an existing weather forecast.";
            operation.Description = "This endpoint allows authenticated users to update an existing weather forecast.";
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "date",
                In = Microsoft.OpenApi.Models.ParameterLocation.Path,
                Required = true,
                Description = "The date of the weather forecast to update.",
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "date"
                }
            });

            // RequestBody Example
            operation.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType>
                {
                    ["application/json"] = new Microsoft.OpenApi.Models.OpenApiMediaType
                    {
                        Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.Schema,
                                Id = "WeatherForecast"
                            }
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["date"] = new Microsoft.OpenApi.Any.OpenApiString("2025-04-24"),
                            ["temperatureC"] = new Microsoft.OpenApi.Any.OpenApiInteger(30),
                            ["summary"] = new Microsoft.OpenApi.Any.OpenApiString("Partly Cloudy")
                        }
                    }
                }
            };

            operation.Responses["200"].Description = "Weather forecast updated successfully.";
            operation.Responses["400"].Description = "Bad request. The input parameters are invalid.";
            operation.Responses["401"].Description = "Unauthorized. The user is not authenticated.";
            operation.Responses["404"].Description = "Not found. The weather forecast for the specified date was not found.";
            operation.Responses["500"].Description = "Internal server error. Something went wrong on the server.";

            return operation;
        });

        // DELETE endpoint (requires authentication)
        endpoints.MapDelete("/weatherforecast/{date}", [Authorize(Policy = "ApiScope")] (DateOnly date) =>
        {
            return Results.NoContent();
        })
        .WithName("DeleteWeatherForecast")
        .Produces(204) // No content
        .Produces(400, typeof(ProblemDetails)) // Bad request
        .Produces(401, typeof(ProblemDetails)) // Unauthorized
        .Produces(404, typeof(ProblemDetails)) // Not found
        .Produces(500, typeof(ProblemDetails)) // Internal server error
        .WithOpenApi(operation =>
        {
            operation.Summary = "Deletes a weather forecast.";
            operation.Description = "This endpoint allows authenticated users to delete a weather forecast by date.";
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "date",
                In = Microsoft.OpenApi.Models.ParameterLocation.Path,
                Required = true,
                Description = "The date of the weather forecast to delete.",
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "date"
                }
            });

            operation.Responses["204"].Description = "No content. The weather forecast was deleted successfully.";
            operation.Responses["400"].Description = "Bad request. The input parameters are invalid.";
            operation.Responses["401"].Description = "Unauthorized. The user is not authenticated.";
            operation.Responses["404"].Description = "Not found. The weather forecast for the specified date was not found.";
            operation.Responses["500"].Description = "Internal server error. Something went wrong on the server.";
            
            return operation;
        });
    }
}

/// <summary>
/// Represents a weather forecast.
/// </summary>
public record WeatherForecast(
    DateOnly Date,
    [Range(-50, 50)] int TemperatureC,
    [StringLength(100)] string? Summary)
{
    /// <summary>
    /// Gets the temperature in Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}