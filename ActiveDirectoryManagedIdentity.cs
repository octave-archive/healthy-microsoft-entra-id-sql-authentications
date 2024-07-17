using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

class ProgramTest1
{
  static void MainTest1(string[] args)
  {
    string database = "OctaveSUIVI";
    string query = "SELECT GETDATE() AS TimeOfQuery";
    string clientId = "3ba5f7c0-6db8-47b0-adaa-ba0cd198f295";
    string server = "octavesigma-devops-managed-mssql.5f77c52f95df.privatelink.database.windows.net";

    // Define the connection string
    string connectionString = $"Server={server};Authentication=Active Directory Managed Identity;Encrypt=True;User Id={clientId};Database={database};TrustServerCertificate=True;";

    // Create and open the SQL connection

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
      conn.Open();

      // Create the SQL command
      using (SqlCommand cmd = conn.CreateCommand())
      {
        cmd.CommandText = query;

        // Execute the command and process the results
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            var timeOfQuery = reader["TimeOfQuery"];
            Console.WriteLine($"Time of Query: {timeOfQuery}");
          }
        }
      }
    }
  }
}

// class Program
// {
//   static async Task Main(string[] args)
//   {
//     string tenantId = "8fb1eb74-3a2a-444e-bfbf-a12f22830e34";
//     string clientId = "<Your-Application-Client-ID>";
//     string authority = $"https://login.microsoftonline.com/{tenantId}";
//     string[] scopes = ["https://database.windows.net/.default"];
//     string database = "OctaveSUIVI";
//     string server = "octavesigma-devops-managed-mssql.5f77c52f95df.privatelink.database.windows.net";
//     string query = "SELECT GETDATE() AS TimeOfQuery";

//     // Use InteractiveBrowserCredential for interactive authentication
//     var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
//     {
//       ClientId = clientId,
//       TenantId = tenantId,
//       AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
//       RedirectUri = new Uri("http://localhost")
//     });

//     var tokenRequestContext = new TokenRequestContext(scopes);
//     var tokenResult = await credential.GetTokenAsync(tokenRequestContext);
//     Console.WriteLine("Token acquired");

//     // Use the token to authenticate to the SQL database
//     var builder = new SqlConnectionStringBuilder
//     {
//       DataSource = server,
//       InitialCatalog = database,
//       ConnectTimeout = 30
//     };

//     using (var connection = new SqlConnection(builder.ConnectionString))
//     {
//       connection.AccessToken = tokenResult.Token;
//       await connection.OpenAsync();
//       Console.WriteLine("Connected to database");

//       using (var command = new SqlCommand(query, connection))
//       {
//         var reader = await command.ExecuteReaderAsync();
//         while (await reader.ReadAsync())
//         {
//           Console.WriteLine($"Time of Query: {reader["TimeOfQuery"]}");
//         }
//       }
//     }
//   }
// }