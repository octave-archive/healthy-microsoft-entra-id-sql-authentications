using System;
using System.IO;
using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Requests;
using System.Threading;
using Microsoft.Identity;
using System.Threading.Tasks;

class Program
{
    private const string OrganizationsTenant = "organizations";
    private const string TOKEN_CACHE_NAME = "YourTokenCacheName";
    private const string clientId = "6e32bb23-677a-4647-9222-2efa6f110d1c";
    private const string tenantId = "8fb1eb74-3a2a-444e-bfbf-a12f22830e34";

    static async Task Main(string[] args)
    {
        // Dummy Main function for demonstration
        Console.WriteLine("Dummy Main Function");

        var cancellationToken = new CancellationToken();

        var token = Authenticate(cancellationToken);
    }

    public static async Task<IAccessToken> Authenticate(CancellationToken cancellationToken)
    {
        var tenantId = "parameters.TenantId";
        var clientId = Constants.PowerShellClientId;
        var scopes = new[] { "User.Read", "Mail.Read", };
        var interactiveParameters = parameters as InteractiveParameters;
        var tokenCacheProvider = interactiveParameters.TokenCacheProvider;
        var resource = interactiveParameters.Environment.GetEndpoint(interactiveParameters.ResourceId) ?? interactiveParameters.ResourceId;

        var requestContext = new TokenRequestContext(scopes, isCaeEnabled: true);
        var authority = interactiveParameters.Environment.ActiveDirectoryAuthority;

        // var options = new InteractiveBrowserCredentialOptions()
        // {
        //     ClientId = clientId,
        //     TenantId = tenantId,
        //     LoginHint = "UserId",
        //     AuthorityHost = new Uri(authority),
        //     RedirectUri = new Uri("http://localhost"),
        //     TokenCachePersistenceOptions = tokenCacheProvider.GetTokenCachePersistenceOptions(),
        // };

        var options = new InteractiveBrowserCredentialOptions
        {
            LoginHint = "",
            TenantId = tenantId,
            ClientId = clientId,
            RedirectUri = new Uri("http://localhost"),
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = TOKEN_CACHE_NAME
            },
        };

        var browserCredential = new InteractiveBrowserCredential(options);

        TracingAdapter.Information($"{DateTime.Now:T} - [InteractiveUserAuthenticator] Calling InteractiveBrowserCredential.AuthenticateAsync with TenantId:'{options.TenantId}', Scopes:'{string.Join(",", scopes)}', AuthorityHost:'{options.AuthorityHost}', RedirectUri:'{options.RedirectUri}'");
        var authTask = browserCredential.AuthenticateAsync(requestContext, cancellationToken);

        return await MsalAccessToken.GetAccessTokenAsync(
            authTask,
            browserCredential,
            requestContext,
            cancellationToken);
    }

    public async Task ReceiveAsync(
        string authFile,
        Func<string, string, DateTimeOffset?, byte[], Task> saveAttachment,
        Func<string, Task<bool>> needDownload,
        CancellationToken cancellationToken
    )
    {
        var scopes = new[] { "User.Read", "Mail.Read", };
        var tenantId = "common";

        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            RedirectUri = new Uri("http://localhost"),
            LoginHint = "",
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = TOKEN_CACHE_NAME
            },
        };

        TokenCredential credential;
        InteractiveBrowserCredential browserCredential;
        AuthenticationRecord authRecord;

        if (!File.Exists(authFile))
        {
            browserCredential = new InteractiveBrowserCredential(options);
            authRecord = await browserCredential.AuthenticateAsync(new TokenRequestContext(scopes), cancellationToken);

            using (var authRecordStream = new FileStream(authFile, FileMode.Create, FileAccess.Write))
            {
                await authRecord.SerializeAsync(authRecordStream);
            }

            credential = browserCredential;
        }
        else
        {
            using (var authRecordStream = new FileStream(authFile, FileMode.Open, FileAccess.Read))
            {
                authRecord = await AuthenticationRecord.DeserializeAsync(authRecordStream);
                options.AuthenticationRecord = authRecord;
                browserCredential = new InteractiveBrowserCredential(options);
                credential = browserCredential;
            }
        }

        var graphClient = new GraphServiceClient(credential, scopes);

        // var msgs = await graphClient.Me.Messages.ToGetRequestInformation().Expand("attachments").GetAsync();

        // while (true)
        // {
        //     if (msgs.CurrentPage != null)
        //     {
        //         foreach (var msg in msgs.CurrentPage)
        //         {
        //             if (!(await needDownload(msg.Id)))
        //             {
        //                 return;
        //             }

        //             Console.WriteLine($"{msg.LastModifiedDateTime} {msg.Subject} ({msg.Attachments?.Count})");

        //             if (msg.HasAttachments == true)
        //             {
        //                 var attachments = msg.Attachments;

        //                 while (true)
        //                 {
        //                     if (attachments?.CurrentPage != null)
        //                     {
        //                         foreach (var attachment in attachments.CurrentPage.OfType<FileAttachment>())
        //                         {
        //                             Console.WriteLine($"- {attachment.Name} {attachment.ContentBytes?.Length}");
        //                             if (attachment.ContentBytes != null)
        //                             {
        //                                 await saveAttachment(
        //                                     msg.Subject,
        //                                     attachment.Name,
        //                                     attachment.LastModifiedDateTime,
        //                                     attachment.ContentBytes
        //                                 );
        //                             }
        //                         }
        //                     }

        //                     if (attachments?.NextPageRequest == null)
        //                     {
        //                         break;
        //                     }

        //                     attachments = await attachments.NextPageRequest.GetAsync();
        //                 }
        //             }
        //         }
        //     }

        //     if (msgs.NextPageRequest == null)
        //     {
        //         return;
        //     }

        //     msgs = await msgs.NextPageRequest.GetAsync();
        // }
    }

    // Placeholder for missing classes and interfaces
    public class AuthenticationParameters { }
    public class InteractiveParameters : AuthenticationParameters { public dynamic Environment; public string TenantId; public string ResourceId; public TokenCacheProvider TokenCacheProvider; public PromptAction PromptAction; public string UserId; }
    public interface IAccessToken { }
    public class MsalAccessToken { public static Task<IAccessToken> GetAccessTokenAsync(Task<AuthenticationRecord> authTask, InteractiveBrowserCredential browserCredential, TokenRequestContext requestContext, CancellationToken cancellationToken) { return null; } }
    public class Constants { public static string PowerShellClientId = "YourPowerShellClientId"; }
    public class TokenCacheProvider { public TokenCachePersistenceOptions GetTokenCachePersistenceOptions() { return null; } }
    public class TracingAdapter { public static void Information(string message) { } }
    public enum PromptAction { }
}