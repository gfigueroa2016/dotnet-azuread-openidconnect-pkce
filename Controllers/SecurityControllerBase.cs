using AzureAD.DotNet.Helpers.Security.Core;
using AzureAD.DotNet.Helpers.Security.Core.Identity;
using System;
using System.Web.Mvc;

namespace AzureAD.DotNet.Controllers
{
	public abstract class SecurityControllerBase : Controller
	{
		protected IUserIdentity UserIdentity { get; set; }
		protected IAppSensor AppSensor { get; set; }

		protected SecurityControllerBase(IUserIdentity userIdentity, IAppSensor appSensor)
		{
			UserIdentity = userIdentity ?? throw new ArgumentNullException(nameof(userIdentity));
			AppSensor = appSensor ?? throw new ArgumentNullException(nameof(appSensor));
		}
	}
}