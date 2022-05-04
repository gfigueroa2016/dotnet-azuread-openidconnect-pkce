using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;

namespace AzureAD.DotNet.Helpers.Security.OIDC
{
    public class SameSiteCookieManager : ICookieManager
    {
        private readonly ICookieManager _innerManager;

        public SameSiteCookieManager() : this(new CookieManager())
        {
        }

        public SameSiteCookieManager(ICookieManager innerManager)
        {
            _innerManager = innerManager;
        }

        public void AppendResponseCookie(IOwinContext context, string key, string value,
                                         CookieOptions options)
        {
            CheckSameSite(context, options);
            _innerManager.AppendResponseCookie(context, key, value, options);
        }

        public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
        {
            CheckSameSite(context, options);
            _innerManager.DeleteCookie(context, key, options);
        }

        public string GetRequestCookie(IOwinContext context, string key)
        {
            return _innerManager.GetRequestCookie(context, key);
        }

        private void CheckSameSite(IOwinContext context, CookieOptions options)
        {
            if (options.SameSite == Microsoft.Owin.SameSiteMode.None
                                 && DisallowsSameSiteNone(context))
            {
                options.SameSite = null;
            }
        }

        public static bool DisallowsSameSiteNone(IOwinContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"];
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }
            if (userAgent.Contains("CPU iPhone OS 12") ||
                userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }
            return false;
        }
    }
}