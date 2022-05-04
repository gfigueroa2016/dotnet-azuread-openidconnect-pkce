using System.Configuration;
using System.Globalization;

namespace AzureAD.DotNet.Helpers.Security.OIDC
{
    public static class AuthenticationConfig
    {
        public const string IssuerClaim = "iss";
        public const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string MicrosoftGraphGroupsApi = "https://graph.microsoft.com/v1.0/groups";
        public const string MicrosoftGraphUsersApi = "https://graph.microsoft.com/v1.0/users";
        public const string AdminConsentFormat = "https://login.microsoftonline.com/{0}/adminconsent?client_id={1}&state={2}&redirect_uri={3}";
        public const string BasicSignInScopes = "openid profile offline_access";
        public const string NameClaimType = "name";
        public static string ClientId { get; } = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string ClientSecret { get; } = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public static string RedirectUri { get; } = ConfigurationManager.AppSettings["ida:RedirectUri"];
        public static string Authority { get; } = string.Format(CultureInfo.InvariantCulture, ConfigurationManager.AppSettings["ida:AADInstance"], ConfigurationManager.AppSettings["ida:TenantId"]);
    }
}