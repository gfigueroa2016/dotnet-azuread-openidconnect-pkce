using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Security.Claims;
using System.Web.Helpers;
using AzureAD.DotNet.Controllers;
using AzureAD.DotNet.App_Start;
using AzureAD.DotNet.Helpers.Security.Core.Identity;
using AzureAD.DotNet.Helpers.Security.Core;
using System.Globalization;

namespace AzureAD.DotNet
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            DependencyInjection.Configure();
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = ((MvcApplication)sender).Context;
            var currentController = " ";
            var currentAction = " ";
            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }

                if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }

            var ex = Server.GetLastError();
            // TODO: Remove service location antipattern
            var controller = new ErrorController(DependencyInjection.Container.GetInstance<IUserIdentity>(), DependencyInjection.Container.GetInstance<IAppSensor>());
            var routeData = new RouteData();
            var action = "Index";

            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "NotFound";
                        break;
                    case 403:
                        action = "Forbidden";
                        break;
                }
            }
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException exception ? exception.GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;
            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        protected void Application_BeginRequest()
        {
#if !DEBUG
            // SECURE: Ensure any request is returned over SSL in production
            if (!Request.IsLocal && !Context.Request.IsSecureConnection) {
                var redirect = Context.Request.Url.ToString().ToLower(CultureInfo.CurrentCulture).Replace("http:", "https:");
                Response.Redirect(redirect);
            }
#endif
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // SECURE: Adding a variable to session keeps the session id constant between requests which is useful for logging and identifying a user between requests
            HttpContext.Current.Session.Add("__MyAppSession", string.Empty);
            // SECURE: Counter CSRF using SameSite https://www.sjoerdlangkemper.nl/2016/04/14/preventing-csrf-with-samesite-cookie-attribute/. Allow Get requests, deny posts
            HttpContext.Current.Response.Cookies["ASP.NET_SessionId"].SameSite = SameSiteMode.Lax;
            CreateUsageLog();
        }
    }
}