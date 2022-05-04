using System;
using System.Web;
using System.Web.Mvc;
using AzureAD.DotNet.Models;
using AzureAD.DotNet.Helpers;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

namespace AzureAD.DotNet.Controllers
{
	[Authorize]
    public class AccountController : Controller
    {
        public async Task Logout()
        {
            await MsalAppBuilder.RemoveAccount();
            HttpContext.GetOwinContext().Authentication.SignOut(
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}
