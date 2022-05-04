using Microsoft.Identity.Client;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureAD.DotNet.Helpers.Security.OIDC
{
    /// <summary>
    /// A simple custom middleware, which takes care of intercepting messages containing authorization codes, validating them, redeeming the code
    /// and saving the resulting tokens in a MSAL cache, and finally redirecting to the URL that originated the request.
    /// </summary>
    /// <seealso cref="Microsoft.Owin.OwinMiddleware" />
    public sealed class OAuth2CodeRedeemerMiddleware : OwinMiddleware
    {
        private readonly OAuth2CodeRedeemerOptions options;
        public OAuth2CodeRedeemerMiddleware(OwinMiddleware next, OAuth2CodeRedeemerOptions options)
            : base(next)
        {
            this.options = options ?? throw new ArgumentNullException("options");
            this.options = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            string code = context.Request.Query["code"];
            if (code != null)
            {
                string state = HttpUtility.UrlDecode(context.Request.Query["state"]);
                string signedInUserID = context.Authentication.User.FindFirst(System.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
                HttpContextBase hcb = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;
                CodeRedemptionData crd = OAuth2RequestManager.ValidateState(state, hcb);
                if (crd != null)
                {
                    IConfidentialClientApplication cc = MsalAppBuilder.BuildConfidentialClientApplication();
                    AuthenticationResult result = await cc.AcquireTokenByAuthorizationCode(crd.Scopes, code).ExecuteAsync().ConfigureAwait(false);
                    context.Response.StatusCode = 302;
                    context.Response.Headers.Set("Location", crd.RequestOriginatorUrl);
                }
                else
                {
                    context.Response.StatusCode = 302;
                    context.Response.Headers.Set("Location", "/Error?message=" + "code_redeem_failed");
                }
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }

    public sealed class OAuth2CodeRedeemerOptions
    {
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string ClientSecret { get; set; }
    }

    internal static class OAuth2CodeRedeemerHandler
    {
        public static IAppBuilder UseOAuth2CodeRedeemer(this IAppBuilder app, OAuth2CodeRedeemerOptions options)
        {
            app.Use<OAuth2CodeRedeemerMiddleware>(options);
            return app;
        }
    }

    #region OIDC

    public class OAuth2RequestManager
    {
        public const string ConsumerTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

        private static readonly ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static string GenerateState(string requestUrl, HttpContextBase httpcontext, UrlHelper url, string[] scopes)
        {
            try
            {
                string stateGuid = Guid.NewGuid().ToString();
                SaveUserStateValue(stateGuid, httpcontext);

                List<String> stateList = new List<string>
                {
                    stateGuid,
                    requestUrl
                };
                string scopeslist = scopes[0];
                if (scopes.Count() > 1)
                {
                    for (int i = 1; i < scopes.Count(); i++)
                    {
                        scopeslist += "," + scopes[i];
                    }
                }

                stateList.Add(scopeslist);

                using (MemoryStream memoryStream = new MemoryStream())
                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    DataContractSerializer serializer = new DataContractSerializer(stateList.GetType());
                    serializer.WriteObject(memoryStream, stateList);
                    memoryStream.Position = 0;
                    var stateBits = Encoding.ASCII.GetBytes(reader.ReadToEnd());
                    return url.Encode(Convert.ToBase64String(stateBits));
                }
            }
            catch
            {
                return null;
            }
        }

        private static void SaveUserStateValue(string stateGuid, HttpContextBase httpcontext)
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(System.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
            SessionLock.EnterWriteLock();
            httpcontext.Session[signedInUserID + "_state"] = stateGuid;
            SessionLock.ExitWriteLock();
        }

        private static string ReadUserStateValue(HttpContextBase httpcontext)
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(System.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
            SessionLock.EnterReadLock();
            var stateGuid = (string)httpcontext.Session[signedInUserID + "_state"];
            SessionLock.ExitReadLock();
            return stateGuid;
        }

        public static CodeRedemptionData ValidateState(string state, HttpContextBase httpcontext)
        {
            try
            {
                var stateBits = Convert.FromBase64String(HttpUtility.UrlEncode(state));

                List<string> stateList = new List<string>();
                using (Stream stream = new MemoryStream())
                {
                    stream.Write(stateBits, 0, stateBits.Length);
                    stream.Position = 0;
                    DataContractSerializer deserializer = new DataContractSerializer(stateList.GetType());
                    stateList = (List<String>)deserializer.ReadObject(stream);
                }

                var stateGuid = stateList[0];
                if (stateGuid == ReadUserStateValue(httpcontext))
                {
                    string returnURL = stateList[1];
                    string[] scopes = stateList[2].Split(',');
                    return new CodeRedemptionData()
                    {
                        RequestOriginatorUrl = returnURL,
                        Scopes = scopes
                    };
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GenerateAuthorizationRequestUrl(string[] scopes, IConfidentialClientApplication cca, HttpContextBase httpcontext, UrlHelper url)
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(System.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
            string preferredUsername = ClaimsPrincipal.Current.FindFirst("preferred_username").Value;
            Uri oauthCodeProcessingPath = new Uri(httpcontext.Request.Url.GetLeftPart(UriPartial.Authority).ToString());
            string state = GenerateState(httpcontext.Request.Url.ToString(), httpcontext, url, scopes);
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            string domain_hint = (tenantID == ConsumerTenantId) ? "consumers" : "organizations";

            Uri authzMessageUri = await cca
                .GetAuthorizationRequestUrl(scopes)
                .WithRedirectUri(oauthCodeProcessingPath.ToString())
                .WithLoginHint(preferredUsername)
                .WithExtraQueryParameters(state == null ? null : "&state=" + state + "&domain_hint=" + domain_hint)
                .WithAuthority(cca.Authority)
                .ExecuteAsync(CancellationToken.None)
                .ConfigureAwait(false);

            return authzMessageUri.ToString();
        }
    }

    public class CodeRedemptionData
    {
        public string RequestOriginatorUrl { get; set; }
        public string[] Scopes { get; set; }
    }

    #endregion OIDC
}