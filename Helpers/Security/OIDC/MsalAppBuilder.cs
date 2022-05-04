using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AzureAD.DotNet.Helpers.Security.OIDC
{
    public static class MsalAppBuilder
    {
        public static string GetAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            string oid = claimsPrincipal.GetObjectId();
            string tid = claimsPrincipal.GetTenantId();
            return $"{oid}.{tid}";
        }

        private static IConfidentialClientApplication clientapp;

        public static IConfidentialClientApplication BuildConfidentialClientApplication()
        {
            if (clientapp == null)
            {
                clientapp = ConfidentialClientApplicationBuilder.Create(AuthenticationConfig.ClientId)
                      .WithClientSecret(AuthenticationConfig.ClientSecret)
                      .WithRedirectUri(AuthenticationConfig.RedirectUri)
                      .WithAuthority(new Uri(AuthenticationConfig.Authority))
                      .Build();

                clientapp.AddDistributedTokenCache(services =>
                {
                    services.AddDistributedMemoryCache();
                    services.Configure<MsalDistributedTokenCacheAdapterOptions>(o =>
                    {
                        o.Encrypt = true;
                    });
                });
            }
            return clientapp;
        }

        public static async Task RemoveAccount()
        {
            BuildConfidentialClientApplication();

            var userAccount = await clientapp.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());
            if (userAccount != null)
            {
                await clientapp.RemoveAsync(userAccount);
            }
        }
    }
}