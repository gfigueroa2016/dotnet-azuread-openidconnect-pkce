using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataTransferObjects;
using AzureAD.DotNet.Models;
using DataTransferObjects.PMFF;
using AzureAD.DotNet.Helpers;
using System.Text.RegularExpressions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Security.Claims;

namespace AzureAD.DotNet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
			if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
            var userClaims = User.Identity as ClaimsIdentity;
            CurrentUser = SecurityUtility.GetAzureADUserAsync(userClaims);
            if (User.Id == default || User.Roles == default)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            return View();

        }
    }
}
