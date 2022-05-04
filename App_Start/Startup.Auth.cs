using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Threading.Tasks;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using PMFF_Web.Helpers.Security.OIDC;
using Serilog;

using Serilog.Sinks.ApplicationInsights;
using Microsoft.IdentityModel.Logging;

namespace AzureAD.DotNet
{
    public partial class Startup
    {
        /// <summary>
        /// Configure OWIN to use OpenIdConnect 
        /// </summary>
        /// <param name="app"></param>
        private void ConfigureAuth(IAppBuilder app)
        {
            IdentityModelEventSource.ShowPII = true;
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOAuth2CodeRedeemer(
                new OAuth2CodeRedeemerOptions
                {
                    ClientId = AuthenticationConfig.ClientId,
                    ClientSecret = AuthenticationConfig.ClientSecret,
                    RedirectUri = AuthenticationConfig.RedirectUri
                });
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    UsePkce = true,
                    ResponseType = OpenIdConnectResponseType.Code,
                    Authority = AuthenticationConfig.Authority,
                    ClientId = AuthenticationConfig.ClientId,
                    RedirectUri = AuthenticationConfig.RedirectUri,
                    PostLogoutRedirectUri = AuthenticationConfig.RedirectUri,
                    Scope = OpenIdConnectScope.OpenIdProfile + " email User.Read",
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        IssuerValidator = (issuer, token, tvp) =>
                        {
                            if (ValidateIssuerWithPlaceholder(issuer, token, tvp))
                            {
                                return issuer;
                            }
                            else
                            {
                                throw new SecurityTokenInvalidIssuerException("Invalid issuer");
                            }
                        }
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        AuthenticationFailed = OnAuthenticationFailed,
                        RedirectToIdentityProvider = OnRedirectToIdentityProvider,
                    },
                    CookieManager = new SameSiteCookieManager(
                                     new SystemWebCookieManager()),
                }
            );
        }

    private Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> arg)
        {
            arg.ProtocolMessage.SetParameter("myNewParameter", "its Value");
            return Task.CompletedTask;
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            context.HandleCodeRedemption();
            context.TokenEndpointRequest.Parameters.TryGetValue("code_verifier", out var codeVerifier);
            IConfidentialClientApplication clientApp = MsalAppBuilder.BuildConfidentialClientApplication();
            AuthenticationResult result = await clientApp.AcquireTokenByAuthorizationCode(new[] { "email User.Read" }, context.Code)
                .WithSpaAuthorizationCode()
                .WithPkceCodeVerifier(codeVerifier)
                .ExecuteAsync();
            HttpContext.Current.Session.Add("Spa_Auth_Code", result.SpaAuthCode);
            context.TokenEndpointResponse.AccessToken = result.AccessToken;
            context.TokenEndpointResponse.IdToken = result.IdToken;
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("Error/Index?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }

        private static bool ValidateIssuerWithPlaceholder(string issuer, SecurityToken token, TokenValidationParameters parameters)
        {
            if (token is JwtSecurityToken jwt)
            {
                if (jwt.Payload.TryGetValue("tid", out var value) &&
                    value is string tokenTenantId)
                {
                    if ((parameters.ValidIssuers ?? Enumerable.Empty<string>())
                        .Append(parameters.ValidIssuer)
                        .Where(i => !string.IsNullOrEmpty(i)).Any(i => i.Replace("{tenantid}", tokenTenantId) == issuer))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}