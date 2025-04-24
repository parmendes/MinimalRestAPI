using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// A filter that checks for the presence of the [Authorize] attribute and applies security requirements to OpenAPI operations.
/// </summary>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies the authorization filter to the OpenAPI operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to modify.</param>
    /// <param name="context">The context for the operation filter.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has the [Authorize] attribute
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true
                           || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            if (operation.Security == null)
                operation.Security = new List<OpenApiSecurityRequirement>();

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { } // No specific scopes required
                }
            });
        }
    }
}