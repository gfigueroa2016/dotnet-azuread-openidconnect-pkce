using System;
using System.Linq;
using System.Configuration;
using AzureAD.DotNet.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using AzureAD.DotNet.Helpers.Security.OIDC;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using System.Collections;
using System.Collections.Generic;

namespace AzureAD.DotNet.Helpers
{
    public class SecurityClaims
    {
        private static readonly string roles = ConfigurationManager.AppSettings["ida:Roles"];
		
		//This is just an example of how you could extract the user claims, is not the only way of doing it.
		//Be sure to implement the best performing methods or functions.
		
        public static User GetAzureADUserAsync(ClaimsIdentity userClaims)
        {
            var jwtPayLoad = new JwtPayload(userClaims.Claims);
            var user = new User
            {
                LifeCycleName = GetPublixEnvironment()
            };
            user.Roles = jwtPayLoad.TryGetValue(ClaimConstants.Roles, out object roles) == true ? roles.GetType().ToString() == "System.String" ? new List<string> { roles.ToString() } : roles.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]" ? (roles as IEnumerable<object>).Cast<string>().ToList() : default : default;
            user.Id = jwtPayLoad.TryGetValue(ClaimConstants.SamAccountName, out object userId) == true ? userId.ToString() : default;
            user.FirstName = jwtPayLoad.TryGetValue(ClaimConstants.GivenName, out object firstName) == true ? firstName.ToString().Split(' ').ToArray().FirstOrDefault() : default;
            user.LastName = jwtPayLoad.TryGetValue(ClaimConstants.GivenName, out object lastName) == true ? lastName.ToString().Split(' ').ToArray().LastOrDefault() : default;
            user.JobClassId = jwtPayLoad.TryGetValue(ClaimConstants.JobClassId, out object jobClassId) == true ? Convert.ToInt32(jobClassId) : default;
            user.Division = jwtPayLoad.TryGetValue(ClaimConstants.Division, out object division) == true ? Convert.ToInt32(division) : default;
            
			//This is just an example of getting user roles.
			
			if (!string.IsNullOrEmpty(user.Id) && user.Roles != default)
            {
                if (user.Division > 0)
                    user.Roles.Add("Division=" + user.Division);
                if (user.JobClassId > 0)
                    user.Roles.Add("JobClassId=" + user.JobClassId);
                user.IsAdmin = false;
                if (user.Roles != null && user.Roles.Count > 0)
                {
                    foreach (var item in user.Roles)
                    {
                        if (item.ToUpper() == adminSecurityKey.ToUpper())
                            user.IsAdmin = true;
                    }
                }
            }
            return user;
        }
    }
}