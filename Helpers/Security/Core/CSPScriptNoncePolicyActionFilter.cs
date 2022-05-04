using System.Globalization;
using System.Web.Mvc;

namespace AzureAD.DotNet.Helpers.Security.Core
{
    public class CSPScriptNoncePolicyActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var scriptSource = string.Format(CultureInfo.InvariantCulture, "script-src 'nonce-{0}' 'self'", CSPScriptNoncePolicy.GetScriptNonce());
            filterContext.HttpContext.Response.AddHeader("Content-Security-Policy", scriptSource);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}