using System;
using System.IO;
using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Requests;
using System.Threading;
using Microsoft.Identity;
using System.Threading.Tasks;

class ProgramMain
{
    private const string OrganizationsTenant = "organizations";
    private const string TOKEN_CACHE_NAME = "YourTokenCacheName";
    private const string clientId = "6e32bb23-677a-4647-9222-2efa6f110d1c";
    private const string tenantId = "8fb1eb74-3a2a-444e-bfbf-a12f22830e34";

    static async Task MainProgram(string[] args)
    {
        // Dummy Main function for demonstration
        Console.WriteLine("Dummy Main Function");

        var cancellationToken = CancellationToken.None;

        var token = await Authenticate(cancellationToken);

        var stop = "stop";
    }

    public static async Task<AuthenticationRecord> Authenticate(CancellationToken cancellationToken)
    // public static async Task<AccessToken> Authenticate(CancellationToken cancellationToken)
    {
        var scopes = new[] { "User.Read" };
        var requestContext = new TokenRequestContext(scopes, isCaeEnabled: true);

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
            }
        };

        var browserCredential = new InteractiveBrowserCredential(options);

        // var tokenRequestContext = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }); 
        // var accessTokenResult = await browserCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
        // var accessToken = accessTokenResult.Token;

        var authResult = await browserCredential.AuthenticateAsync(requestContext, cancellationToken);

        // Assuming `authResult` contains the authentication result,
        // you can extract the token and other relevant information from it.
        return authResult;
        // return accessTokenResult;

        // Assuming `tenantId`, `userId`, and `homeAccountId` are defined elsewhere in your code.
        // Create an instance of `MsalAccessToken` or a similar class that you have defined
        // // to encapsulate the token information.
        // var msalAccessToken = new MsalAccessToken(
        //     browserCredential,
        //     requestContext,
        //     token,
        //     expiresOn,
        //     tenantId,
        //     userId,
        //     homeAccountId);
    }
}