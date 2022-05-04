using System.Web.Mvc;

namespace AzureAD.DotNet.Helpers.HtmlHelpers
{
    public static class HtmlHelpers
    {
        public static bool IsReleaseBuild(this HtmlHelper helper)
        {
#if DEBUG
            return false;
#else
    return true;
#endif
        }
    }
}