using AzureAD.DotNet.Helpers.Security.Core;
using AzureAD.DotNet.Helpers.Security.Core.Identity;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using System.Reflection;
using System.Web.Mvc;

namespace AzureAD.DotNet.App_Start
{
    public static class DependencyInjection
    {
		public static Container Container { get; set; }

		public static void Configure()
		{
			// 1. Create a new Simple Injector container
			var container = new Container();
			container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

			container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
			container.RegisterMvcIntegratedFilterProvider();

			// 2. Configure the container (register)
			container.Register<IAppSensor, AppSensor>(Lifestyle.Scoped);
			container.Register<IUserIdentity, UserIdentity>(Lifestyle.Scoped);

			// 3. Optionally verify the container's configuration.
			container.Verify();

			// 4. Register the container as MVC3 IDependencyResolver.
			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
			Container = container;
		}
	}
}