using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureAD.DotNet.Helpers.Security.OIDC
{
    public interface IAuthenticationConfig
    {
        string ClientId { get; }
        string ClientSecret { get; }
        string RedirectUri { get; }
        string Authority { get; }
    }
}