using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using AzureAD.DotNet.Models;

namespace AzureAD.DotNet.Helpers.Security.OIDC
{
    public static class GraphHelper
    {
        public static async Task<CachedUser> GetUserDetailsAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) => {
                        requestMessage.Headers.Authorization =
                           new AuthenticationHeaderValue("Bearer", accessToken);
                        return Task.CompletedTask;
                    }));

            var user = await graphClient.Me.Request()
                .Select(u => new {
                    u.DisplayName,
                    u.Mail,
                    u.UserPrincipalName
                })
                .GetAsync();

            return new CachedUser
            {
                DisplayName = user.DisplayName,
                Email = string.IsNullOrEmpty(user.Mail) ?
                    user.UserPrincipalName : user.Mail
            };
        }

        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var events = await graphClient.Me.Events.Request()
                .Select("subject,organizer,start,end")
                .OrderBy("createdDateTime DESC")
                .GetAsync();

            return events.CurrentPage;
        }

        private static GraphServiceClient GetAuthenticatedClient()
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var idClient = ConfidentialClientApplicationBuilder.Create(AuthenticationConfig.ClientId)
                            .WithRedirectUri(AuthenticationConfig.RedirectUri)
                            .WithClientSecret(AuthenticationConfig.ClientSecret)
                            .Build();

                        var tokenStore = new SessionTokenStore(idClient.UserTokenCache,
                                HttpContext.Current, ClaimsPrincipal.Current);

                        var userUniqueId = tokenStore.GetUsersUniqueId(ClaimsPrincipal.Current);
                        var account = await idClient.GetAccountAsync(userUniqueId);

                        // By calling this here, the token can be refreshed
                        // if it's expired right before the Graph call is made
                        var result = await idClient.AcquireTokenSilent("openid profile email User.Read".Split(' '), account)
                                    .ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
        }
    }
}