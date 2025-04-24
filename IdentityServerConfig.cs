using Duende.IdentityServer;
using Duende.IdentityServer.Models;

/// <summary>
/// Provides configuration for IdentityServer, including clients, API scopes, and identity resources.
/// </summary>
public static class IdentityServerConfig
{
    /// <summary>
    /// Gets the list of clients that can access the IdentityServer.
    /// </summary>
    /// <returns>A collection of <see cref="Client"/> objects.</returns>
    public static IEnumerable<Client> GetClients() =>
        new List<Client>
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
        };

    /// <summary>
    /// Gets the list of API scopes available in the IdentityServer.
    /// </summary>
    /// <returns>A collection of <see cref="ApiScope"/> objects.</returns>
    public static IEnumerable<ApiScope> GetApiScopes() =>
        new List<ApiScope>
        {
            new ApiScope("api1", "MinimalRestAPI")
        };

    /// <summary>
    /// Gets the list of identity resources available in the IdentityServer.
    /// </summary>
    /// <returns>A collection of <see cref="IdentityResource"/> objects.</returns>
    public static IEnumerable<IdentityResource> GetIdentityResources() =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };
}