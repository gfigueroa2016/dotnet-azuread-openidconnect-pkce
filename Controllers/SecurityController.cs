using Newtonsoft.Json;
using AzureAD.DotNet.Helpers.Security.Core.Attributes;
using AzureAD.DotNet.Helpers.Security.Model;
using System.IO;
using System.Text;
using System.Web.Mvc;

namespace AzureAD.DotNet.Controllers
{
    public class SecurityController : Controller
    {
        [HttpPost]
        [AllowXRequestsEveryXSeconds(Name = "CspReporting", ContentName = "TooManyRequests", Requests = 15, Seconds = 60)]
        public JsonResult CspReporting([ModelBinder(typeof(JsonModelBinder<CspHolder>))] CspHolder data)
        {
            LogMessage($"Content Security Policy Violation {data.CspReport}", 4);
            return Json(new { success = true });
        }

        [HttpPost]
        [AllowXRequestsEveryXSeconds(Name = "HpkpReporting", ContentName = "TooManyRequests", Requests = 15, Seconds = 60)]
        public JsonResult HpkpReporting([ModelBinder(typeof(JsonModelBinder<HpkpReport>))] HpkpReport data)
        {
            LogMessage($"Http Public Key Pinning Violation {data}", 4);
            return Json(new { success = true });
        }

        [HttpPost]
        [AllowXRequestsEveryXSeconds(Name = "CtReporting", ContentName = "TooManyRequests", Requests = 15, Seconds = 60)]
        public JsonResult CtReporting([ModelBinder(typeof(JsonModelBinder<CtHolder>))] CtHolder data)
        {
            LogMessage($"Certificate Transparency Violation {data}", 4);
            return Json(new { success = true });
        }
    }

    internal class JsonModelBinder<T> : IModelBinder where T : class
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            controllerContext.HttpContext.Request.InputStream.Position = 0;
            var stream = controllerContext.RequestContext.HttpContext.Request.InputStream;
            var readStream = new StreamReader(stream, Encoding.UTF8);
            var json = readStream.ReadToEnd();
            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None
            };
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
}