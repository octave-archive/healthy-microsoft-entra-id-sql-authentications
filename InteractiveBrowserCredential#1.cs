using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

class ProgramTest2
{
    private const string OrganizationsTenant = "organizations";
    private const string TOKEN_CACHE_NAME = "YourTokenCacheName";
    private const string clientId = "6e32bb23-677a-4647-9222-2efa6f110d1c";
    private const string tenantId = "8fb1eb74-3a2a-444e-bfbf-a12f22830e34";

    static async Task Main(string[] args)
    {

        string database = "Octave_CDL";
        string query = "SELECT GETDATE() AS TimeOfQuery";
        string server = "octavesigma-devops-managed-mssql.public.5f77c52f95df.database.windows.net,3342";
        // string server = "octavesigma-devops-managed-mssql.5f77c52f95df.privatelink.database.windows.net";

        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            RedirectUri = new Uri("http://localhost"),
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            LoginHint = "mc05.octave@octavesigma.onmicrosoft.com",
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = TOKEN_CACHE_NAME
            },
            Diagnostics =
            {
                IsLoggingEnabled = true,
                LoggedHeaderNames = { "*" },
                IsLoggingContentEnabled = true,
                LoggedQueryParameters = { "*" },
            },
            DisableAutomaticAuthentication = false,
        };

        var browserCredential = new InteractiveBrowserCredential(options);

        string scope = "https://database.windows.net/.default";
        var connectionString = $"Server=tcp:{server}; Database={database}; TrustServerCertificate=True;";
        // var tokenCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { SharedTokenCacheTenantId = tenantId, ExcludeInteractiveBrowserCredential = false });
        var accessToken = await browserCredential.GetTokenAsync(new TokenRequestContext([scope]), CancellationToken.None);

        using (var connection = new SqlConnection(connectionString))
        {
            connection.AccessToken = accessToken.Token;
            await connection.OpenAsync();
            Console.WriteLine("Connected to database");

            using (var command = new SqlCommand(query, connection))
            {
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"Time of Query: {reader["TimeOfQuery"]}");
                }
            }
        }
    }
}