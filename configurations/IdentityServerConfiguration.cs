using Duende.IdentityServer.Models;

/// <summary>
/// This class contains the configuration for IdentityServer, including clients, API scopes, and identity resources.
/// It also sets up the signing credentials for development purposes.
/// </summary>
public static class IdentityServerConfiguration
{
    /// <summary>
    /// This method configures IdentityServer with in-memory clients, API scopes, and identity resources.
    /// It also adds developer signing credentials for development purposes.
    /// </summary>
    /// <param name="services"></param>
    public static void AddCustomIdentityServer(this IServiceCollection services)
    {
        services.AddIdentityServer()
            .AddInMemoryClients(new List<Client>
            {
                new Client
                {
                    ClientId = "minimalrestapi-client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("your-client-secret".Sha256()) },
                    AllowedScopes = { "api1" }
                }
            })
            .AddInMemoryApiScopes(new List<ApiScope>
            {
                new ApiScope("api1", "Access MinimalRestAPI")
            })
            .AddInMemoryIdentityResources(new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            })
            .AddDeveloperSigningCredential();
    }
}
