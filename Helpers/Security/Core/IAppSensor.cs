using System.Collections.Generic;
using System.Web.Mvc;

namespace AzureAD.DotNet.Helpers.Security.Core
{
	public interface IAppSensor
	{
		void ValidateFormData(Controller controller, List<string> expectedFormKeys);
		void InspectModelStateErrors(Controller controller);
	}
}
