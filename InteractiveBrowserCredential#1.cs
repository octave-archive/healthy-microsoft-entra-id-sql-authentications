using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

class ProgramTest
{
    static async Task Main(string[] args)
    {
        string tenantId = "8fb1eb74-3a2a-444e-bfbf-a12f22830e34";
        string clientId = "<Your-Application-Client-ID>";
        string authority = $"https://login.microsoftonline.com/{tenantId}";
        string[] scopes = new string[] { "https://database.windows.net/.default" };
        string database = "OctaveSUIVI";
        string server = "octavesigma-devops-managed-mssql.5f77c52f95df.privatelink.database.windows.net";
        string query = "SELECT GETDATE() AS TimeOfQuery";

        // Use InteractiveBrowserCredential for interactive authentication
        var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            ClientId = clientId,
            TenantId = tenantId,
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            RedirectUri = new Uri("http://localhost")
        });

        var tokenRequestContext = new TokenRequestContext(scopes);
        var tokenResult = await credential.GetTokenAsync(tokenRequestContext);
        Console.WriteLine("Token acquired");

        // Use the token to authenticate to the SQL database
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            ConnectTimeout = 30
        };

        using (var connection = new SqlConnection(builder.ConnectionString))
        {
            connection.AccessToken = tokenResult.Token;
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