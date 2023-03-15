using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleEase.Services
{
    public class AuthenticationService : IAuthenticationProvider
    {
        public GraphServiceClient GraphClient => new GraphServiceClient(this);
        public IPublicClientApplication IdentityClient { get; set; }

        public async Task AuthenticateRequestAsync(
            RequestInformation request,
            Dictionary<string, object> additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default)
        {
            if (request.URI.Host == "graph.microsoft.com")
            {
                var result = await GetAuthenticationToken();

                request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
            }
        }
        public async Task<AuthenticationResult> GetAuthenticationToken()
        {
            if (IdentityClient == null)
            {
#if ANDROID
        IdentityClient = PublicClientApplicationBuilder
            .Create(Helpers.Constants.ApplicationId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithRedirectUri($"msal{Helpers.Constants.ApplicationId}://auth")
            .WithParentActivityOrWindow(() => Platform.CurrentActivity)
            .Build();
#elif IOS
        IdentityClient = PublicClientApplicationBuilder
            .Create(Helpers.Constants.ApplicationId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
            .WithRedirectUri($"msal{Helpers.Constants.ApplicationId}://auth")
            .Build();
#else
                IdentityClient = PublicClientApplicationBuilder
                    .Create(Helpers.Constants.ApplicationId)
                    .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                    .Build();
#endif
            }

            var accounts = await IdentityClient.GetAccountsAsync();
            AuthenticationResult result = null;
            bool tryInteractiveLogin = false;

            try
            {
                result = await IdentityClient
                    .AcquireTokenSilent(Helpers.Constants.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                tryInteractiveLogin = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSAL Silent Error: {ex.Message}");
            }

            if (tryInteractiveLogin)
            {
                try
                {
                    result = await IdentityClient
                        .AcquireTokenInteractive(Helpers.Constants.Scopes)
                        .ExecuteAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MSAL Interactive Error: {ex.Message}");
                }
            }
            return result;
        }
    }
}
